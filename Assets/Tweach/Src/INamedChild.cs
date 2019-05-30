namespace Tweach
{
    public interface INamedChild
    {
        string GetName();
        string GetTypeName();
        INamedChild GetParentAsINamedChild();
    }
}