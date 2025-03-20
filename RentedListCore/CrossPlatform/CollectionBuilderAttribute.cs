#if !NET8_0_OR_GREATER
#pragma warning disable IDE0055 // False positive
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public sealed class CollectionBuilderAttribute(Type builderType, string methodName) : Attribute
{
    public Type BuilderType => builderType;
    public string MethodName => methodName;
}
#endif