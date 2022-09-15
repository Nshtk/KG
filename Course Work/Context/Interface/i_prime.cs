namespace KP.Context.Interface
{
    public interface IPrime<T>
    {
        public bool performTest(T number, float probability_minimal);
    }
}