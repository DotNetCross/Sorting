using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCross.Sorting.Benchmarks
{
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
}
