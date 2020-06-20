# Notes

## JitDasm

https://github.com/0xd4d/JitDasm
```
dotnet tool install -g JitDasm.0xd4d
jitdasm --diffable -p 15804 -m DotNetCross.Sorting --heap-search
```
can't get generic methods though it seems.

## CLR Issues and PRs

TODO: Add PR for managed Sort

TODO: Add PR for fixing int regression e.g. ref loop 

Fix regression in Array.Sort for floats/doubles
https://github.com/dotnet/runtime/pull/37941

## JIT Issues

TODO: JIT cannot inline methods with delegate calls

TODO: Older runtime JITs cannot inline methods with `calli` calls


#### Irrelevant type checks in inlined generics for value types influencing optimizations 

https://github.com/dotnet/runtime/issues/37904

https://sharplab.io/#v2:EYLgxg9gTgpgtADwGwBYA0AXEBDAzgWwB8ABAJgEYBYAKGIAYACY8gOgCUBXAOwwEt8YLAMIR8AB14AbGFADKMgG68wMXAG4a9BrIAW2KGIAy2YO258BG6jQDaAKV4YA4jC4zlACgwBPMTAgAZh68PACUoQC6mgDMDLgw2NIAJkykDEIAPAAqAHwMAO46MjAMWQwgDACSIuL6JtLZOTQA3jQM7UyxzEgMIRjpomL6MOQeskNcjQwA1jDeuGilDBihDAC8eYaquFl6XKOz8zZ0EYsrDAD8DOTlDHRWbR3EXeQ9fQO1sKRjE1OHC0tzhsGFtcDs9t9/sdTstVlcbhV7jRHu0bABZGAYHQQJKVcSSDwYrE4vFiSQAeTEfAgXFwLAAggBzRmwMG8BQwSpcSQhEKMyIozpMV4MYAQCCSEHbXbYfYeMrSAIYRZlKC8Rk6FaC1rUDp63oBBheXz+IJZVZrNbLE2BDzMOjhQX6p4AdgYAUS8QeuudTDdiowwkGwyyEA8ao15wydyseoAvsifaiidjcfjCZjU6SKVTeDS6UyWdt2ZzubyuPyokmhd1ReLJaDwbLvgqYEqVQwI5rQtqna6GAGg58YKHw+ruwxo0jqwnq4L0ZmSemU0uyZTqbSGczWbgS1yeVw+QLq89hT0xRKpWCZVxovKB23lUsu1rqzrfX6H0qh0NYKOX6sU6XMsUAcCUFQepIXqCrOcZAA=

```csharp
using System;
using System.Runtime.CompilerServices;
using SharpLab.Runtime;

[JitGeneric(typeof(int))]
public sealed class C<T> where T : IComparable<T>
{
    public static int Compare1(Span<T> keys, T t) => LessThan1(keys[0], t) ? 1 : 0;

    public static int Compare2(Span<T> keys, T t) => LessThan2(keys[0], t) ? 1 : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThan1(T left, T right)
    {
        if (typeof(T) == typeof(string))
            return false;

        return left.CompareTo(right) < 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThan2(T left, T right)
    {
        return left.CompareTo(right) < 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LessThan3(T left, T right)
    {
        return left.CompareTo(right) < 0 ? true : false;
    }
}
```

```asm
; Core CLR v4.700.20.20201 on amd64

C`1[[System.Int32, System.Private.CoreLib]]..ctor()
    L0000: ret

C`1[[System.Int32, System.Private.CoreLib]].Compare1(System.Span`1<Int32>, Int32)
    L0000: sub rsp, 0x28
    L0004: cmp dword ptr [rcx+8], 0
    L0008: jbe short L0044
    L000a: mov rax, [rcx]
    L000d: mov eax, [rax]
    L000f: cmp eax, edx
    L0011: jge short L001a
    L0013: mov ecx, 0xffffffff
    L0018: jmp short L0027
    L001a: cmp eax, edx
    L001c: jle short L0025
    L001e: mov ecx, 1
    L0023: jmp short L0027
    L0025: xor ecx, ecx
    L0027: test ecx, ecx
    L0029: setl al
    L002c: movzx eax, al
    L002f: test eax, eax
    L0031: jne short L003a
    L0033: xor eax, eax
    L0035: add rsp, 0x28
    L0039: ret
    L003a: mov eax, 1
    L003f: add rsp, 0x28
    L0043: ret
    L0044: call 0x00007ff8aa8af9f0
    L0049: int3

C`1[[System.Int32, System.Private.CoreLib]].Compare2(System.Span`1<Int32>, Int32)
    L0000: sub rsp, 0x28
    L0004: cmp dword ptr [rcx+8], 0
    L0008: jbe short L0024
    L000a: mov rax, [rcx]
    L000d: mov eax, [rax]
    L000f: cmp eax, edx
    L0011: jl short L001a
    L0013: xor eax, eax
    L0015: add rsp, 0x28
    L0019: ret
    L001a: mov eax, 1
    L001f: add rsp, 0x28
    L0023: ret
    L0024: call 0x00007ff8aa8af9f0
    L0029: int3

C`1[[System.Int32, System.Private.CoreLib]].LessThan1(Int32, Int32)
    L0000: cmp ecx, edx
    L0002: jge short L000b
    L0004: mov eax, 0xffffffff
    L0009: jmp short L0018
    L000b: cmp ecx, edx
    L000d: jle short L0016
    L000f: mov eax, 1
    L0014: jmp short L0018
    L0016: xor eax, eax
    L0018: test eax, eax
    L001a: setl al
    L001d: movzx eax, al
    L0020: ret

C`1[[System.Int32, System.Private.CoreLib]].LessThan2(Int32, Int32)
    L0000: cmp ecx, edx
    L0002: jge short L000b
    L0004: mov eax, 0xffffffff
    L0009: jmp short L0018
    L000b: cmp ecx, edx
    L000d: jle short L0016
    L000f: mov eax, 1
    L0014: jmp short L0018
    L0016: xor eax, eax
    L0018: test eax, eax
    L001a: setl al
    L001d: movzx eax, al
    L0020: ret

C`1[[System.Int32, System.Private.CoreLib]].LessThan3(Int32, Int32)
    L0000: cmp ecx, edx
    L0002: jl short L0007
    L0004: xor eax, eax
    L0006: ret
    L0007: mov eax, 1
    L000c: ret
```