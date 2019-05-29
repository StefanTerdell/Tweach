public interface INamedChild
{
    string GetName();
    INamedChild GetParentWithName();
}