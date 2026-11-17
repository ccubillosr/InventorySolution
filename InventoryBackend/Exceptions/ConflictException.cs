namespace InventoryBackend.Exceptions;

/// <summary>Thrown when a client request conflicts with existing business state (e.g. duplicate SKU).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
