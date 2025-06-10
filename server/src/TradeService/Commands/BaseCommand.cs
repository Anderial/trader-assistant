namespace TradeService.Commands;

/// <summary>
/// Базовый класс для всех команд в системе
/// </summary>
[GenerateSerializer]
[Alias(nameof(BaseCommand))]
public abstract class BaseCommand
{
    /// <summary>
    /// Уникальный идентификатор команды
    /// </summary>
    [Id(0)]
    public string Id { get; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Время создания команды
    /// </summary>
    [Id(1)]
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    
    /// <summary>
    /// Тип команды для сериализации/десериализации
    /// </summary>
    [Id(2)]
    public virtual string CommandType { get; protected set; } = string.Empty;
}

/// <summary>
/// Базовый класс для команд с результатом определенного типа
/// </summary>
/// <typeparam name="TResult">Тип результата команды</typeparam>
[GenerateSerializer]
[Alias("TradeService.Commands.BaseCommand`1")]
public abstract class BaseCommand<TResult> : BaseCommand
{
    // Дополнительная логика для типизированных команд при необходимости
} 