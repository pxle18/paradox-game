namespace VMP_CNR.Module
{
    public interface Identifiable<out T>
    {
        T GetIdentifier();
    }
}