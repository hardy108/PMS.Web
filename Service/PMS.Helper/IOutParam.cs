namespace PMS.EFCore.Helper
{
    public interface IOutParam<T>
    {
        T Value { get; }
    }
}
