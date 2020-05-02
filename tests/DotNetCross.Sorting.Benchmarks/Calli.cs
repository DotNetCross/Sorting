using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCross.Sorting.Benchmarks
{
    // https://github.com/dotnet/csharplang/blob/master/proposals/function-pointers.md

    // Asked Tanner Gooding:
    // https://github.com/dotnet/runtime/issues/23062#issuecomment-622456190

    // https://github.com/dotnet/runtime/issues/23062
    // https://github.com/dotnet/corert/blob/master/src/Common/src/TypeSystem/IL/Stubs/CalliIntrinsic.cs
    // Bing uses ldftn + calli: https://devblogs.microsoft.com/dotnet/bing-com-runs-on-net-core-2-1/
    class Calli
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldftn?redirectedfrom=MSDN&view=netcore-3.1
        // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldvirtftn?redirectedfrom=MSDN&view=netcore-3.1
        // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.calli?redirectedfrom=MSDN&view=netcore-3.1
    }
    // https://stackoverflow.com/questions/14006427/how-to-save-a-methodinfo-pointer-and-later-call-that-function/14008352#14008352
    // https://stackoverflow.com/questions/15187039/using-calli-to-invoke-a-member-function
    // https://stackoverflow.com/questions/55534030/msil-opcode-ldftn-vs-runtimemethodhandle

    // https://stackoverflow.com/questions/3527917/call-a-method-using-a-methodinfo-instance-on-the-stack-using-reflection-emit/3528106#3528106
    // don’t pass around MethodInfo instances.You could, for example, 
    // pass managed function pointers instead.Those are the things that the ldftn and ldvirtftn instructions return. 
    // You can then use the calli instruction to invoke one of those. You will need to construct the “call-site description”, 
    // which calli expects as an operand, using the SignatureHelper class.
    // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.signaturehelper?redirectedfrom=MSDN&view=netcore-3.1

    // https://github.com/ltrzesniewski/InlineIL.Fody/blob/master/src/InlineIL.Tests.UnverifiableAssemblyToProcess/StandAloneMethodSigTestCases.cs

    // Series on calls in C#
    // https://blog.adamfurmanek.pl/2016/05/21/virtual-and-non-virtual-calls-in-c/

    // Examples
    // https://github.com/migueldeicaza/mono-wasm-mono/blob/master/mono/tests/metadata-verifier/assembly-with-calli.il
    // .method public static int32 SimpleMethod(int32 foo) cil managed
    // {
    //     ldc.i4.0
    //     ret
    // }
    // .method public static void MethodWithCalli() cil managed
    // {
    //     ldftn int32 class Program::SimpleMethod(int32)
    //     ldc.i4.1
    //     calli unmanaged cdecl int32(int32)
    //     pop
    //     ret
    // }


    // Proposed Unsafe naming schema.

    //public static TResult Calli_[CALLCONV]_Delegate_[Func|Action]_[Open]<T1, T2, TResult>(IntPtr functionPtr, T1 arg1, T2 arg2)
    //{ }

    //public static TResult Calli_Managed_Func_Open<T1, T2, TResult>(IntPtr functionPtr, T1 arg1, T2 arg2)
    //{ }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static TResult DISASSEMBLE<TDelegate, T1, T2, TResult>(TDelegate compare, T1 x, T2 y)
    //     where TDelegate : class, System.Delegate
    //     => Unsafe.As<Func<T1, T2, TResult>>(compare)(x, y);
    //
    // .method public hidebysig static !!TResult
    //         DISASSEMBLE<class ([System.Runtime] System.Delegate) TDelegate,T1,T2,TResult>(!!TDelegate compare,
    //                                                                                      !!T1 x,
    //                                                                                      !!T2 y) cil managed aggressiveinlining
    // {
    //   // Code size       19 (0x13)
    //   .maxstack  8
    //   IL_0000:  ldarg.0
    //   IL_0001:  box        !!TDelegate
    //   IL_0006:  call       !!0 [System.Runtime.CompilerServices.Unsafe] System.Runtime.CompilerServices.Unsafe::As<class [System.Runtime] System.Func`3<!!1,!!2,!!3>>(object)
    //   IL_000b:  ldarg.1
    //   IL_000c:  ldarg.2
    //   IL_000d:  callvirt instance !2 class [System.Runtime] System.Func`3<!!T1,!!T2,!!TResult>::Invoke(!0,
    //                                                                                                     !1)
    //   IL_0012:  ret
    // } // end of method CompareToLessThanBench`1::DISASSEMBLE


    // ```antlr
    // pointer_type
    //     : ...
    //     | funcptr_type
    //     ;
    // 
    // funcptr_type
    //     : 'delegate' '*' calling_convention? '<' (funcptr_parameter_modifier? type ',')* funcptr_return_modifier? return_type '>'
    //     ;
    // 
    // calling_convention
    //     : 'cdecl'
    //     | 'managed'
    //     | 'stdcall'
    //     | 'thiscall'
    //     | 'unmanaged'
    //     ;
    // 
    // funcptr_parameter_modifier
    //     : 'ref'
    //     | 'out'
    //     | 'in'
    //     ;
    // 
    // funcptr_return_modifier
    //     : 'ref'
    //     | 'ref readonly'
    //     ;
    // ```
}
