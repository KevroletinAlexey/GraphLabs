namespace Domain.VariantData;

public sealed class VariantData<T>
{
    public VariantDataType Type { get; set; }
        
    public T Value { get; set; } 
}