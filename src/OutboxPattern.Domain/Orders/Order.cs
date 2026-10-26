namespace OutboxPattern.Domain.Orders;

/// <summary>
/// Заказ покупателя. Хранит состояние заказа и контролирует допустимые переходы статусов.
/// </summary>
public sealed class Order
{
    /// <summary>
    /// Максимальная длина адреса электронной почты покупателя.
    /// </summary>
    /// <remarks>
    /// Значение используется в EF-конфигурации и должно совпадать
    /// с ограничением колонки в базе данных.
    /// </remarks>
    public const int CustomerEmailMaxLength = 255;

    // Закрытый конструктор запрещает создание заказа в обход Order.Create.
    private Order()
    {
    }

    /// <summary>
    /// Уникальный идентификатор заказа.
    /// </summary>
    /// <remarks>
    /// Создаётся как UUID v7, поэтому идентификаторы упорядочены по времени создания.
    /// </remarks>
    public Guid Id { get; private set; }

    /// <summary>
    /// Адрес электронной почты покупателя.
    /// </summary>
    public string CustomerEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Итоговая сумма заказа.
    /// </summary>
    public decimal TotalPrice { get; private set; }

    /// <summary>
    /// Текущий статус заказа.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Дата и время создания заказа в UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>
    /// Создаёт заказ в статусе <see cref="OrderStatus.Pending"/>.
    /// </summary>
    /// <param name="customerEmail">
    /// Адрес электронной почты покупателя. Длина не должна превышать
    /// <see cref="CustomerEmailMaxLength"/> символов.
    /// </param>
    /// <param name="totalPrice">
    /// Итоговая сумма заказа. Должна быть больше нуля.
    /// </param>
    /// <remarks>
    /// Пробелы в начале и в конце адреса удаляются перед сохранением.
    /// Проверка адреса упрощённая: достаточно наличия символа '@'.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Возникает, если адрес электронной почты пуст, не содержит символ '@'
    /// или превышает допустимую длину.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Возникает, если итоговая сумма заказа меньше или равна нулю.
    /// </exception>
    public static Order Create(
        string customerEmail,
        decimal totalPrice)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new ArgumentException(
                "Адрес электронной почты покупателя обязателен.",
                nameof(customerEmail));
        }

        customerEmail = customerEmail.Trim();

        if (customerEmail.Length > CustomerEmailMaxLength)
        {
            throw new ArgumentException(
                $"Адрес электронной почты не должен превышать {CustomerEmailMaxLength} символов.",
                nameof(customerEmail));
        }

        if (!customerEmail.Contains('@'))
        {
            throw new ArgumentException(
                "Адрес электронной почты должен содержать символ '@'.",
                nameof(customerEmail));
        }

        if (totalPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalPrice),
                totalPrice,
                "Итоговая сумма заказа должна быть больше нуля.");
        }

        return new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerEmail = customerEmail,
            TotalPrice = totalPrice,
            Status = OrderStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Переводит заказ из статуса <see cref="OrderStatus.Pending"/>
    /// в <see cref="OrderStatus.Confirmed"/>.
    /// </summary>
    /// <remarks>
    /// Переход необратим: подтверждённый заказ нельзя подтвердить повторно или отменить.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Возникает, если заказ не находится в статусе ожидания.
    /// </exception>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                "Нельзя подтвердить заказ, который не находится в статусе ожидания.");
        }

        Status = OrderStatus.Confirmed;
    }

    /// <summary>
    /// Переводит заказ из статуса <see cref="OrderStatus.Pending"/>
    /// в <see cref="OrderStatus.Cancelled"/>.
    /// </summary>
    /// <remarks>
    /// Переход необратим: отменённый заказ нельзя отменить повторно или подтвердить.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Возникает, если заказ не находится в статусе ожидания.
    /// </exception>
    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                "Нельзя отменить заказ, который не находится в статусе ожидания.");
        }

        Status = OrderStatus.Cancelled;
    }
}