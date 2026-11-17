namespace InventoryBackend.Extensions;

/// <summary>
/// Extensiones para reducir duplicación de código en servicios.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Convierte una entidad a DTO de forma segura, manejando null.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad</typeparam>
    /// <typeparam name="TDto">Tipo del DTO</typeparam>
    /// <param name="entity">Entidad a convertir (puede ser null)</param>
    /// <param name="mapper">Función que mapea T a TDto</param>
    /// <returns>DTO mapeado o null</returns>
    public static TDto? MapToDto<T, TDto>(this T? entity, Func<T, TDto> mapper) where T : class
    {
        return entity is null ? default : mapper(entity);
    }

    /// <summary>
    /// Realiza un Trim seguro de string, retornando el valor original si es null o vacío.
    /// </summary>
    /// <param name="value">String a limpiar</param>
    /// <returns>String limpiado o null/vacío según entrada</returns>
    public static string? SafeTrim(this string? value)
    {
        return string.IsNullOrEmpty(value) ? value : value.Trim();
    }

    /// <summary>
    /// Convierte una lista a IReadOnlyList sin materializar nuevamente si ya está materializada.
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos</typeparam>
    /// <param name="collection">Colección a convertir</param>
    /// <returns>IReadOnlyList</returns>
    public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> collection)
    {
        return collection is IReadOnlyList<T> roList 
            ? roList 
            : collection.ToList().AsReadOnly();
    }
}
