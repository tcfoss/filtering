namespace TcfOss.Filtering.Contracts;

public class FilterDeserializationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
