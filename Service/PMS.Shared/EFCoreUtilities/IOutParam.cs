namespace PMS.Shared.EFCoreUtilities
{
    public interface IOutParam<T>
    {
        T Value { get; }
    }
}
