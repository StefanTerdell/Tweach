namespace Tweach
{
    public interface INamedChild
    {
        string GetName();
        INamedChild GetParentAsINamedChild();
    }
}