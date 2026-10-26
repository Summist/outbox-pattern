using OutboxPattern.Domain.Orders;

namespace OutboxPattern.UnitTests.Domain.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Create_ValidData_ReturnsPendingOrder()
    {
        // Arrange
        const string email = "customer@example.com";
        const decimal price = 1500m;

        // Act
        var order = Order.Create(email, price);

        // Assert
        Assert.Equal(email, order.CustomerEmail);
        Assert.Equal(price, order.TotalPrice);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.NotEqual(Guid.Empty, order.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("customer.com")]
    public void Create_InvalidEmail_ThrowsArgumentException(string? email)
    {
        // Arrange
        const decimal price = 1500m;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => Order.Create(email!, price));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_InvalidPrice_ThrowsArgumentOutOfRangeException(decimal price)
    {
        // Arrange
        const string email = "customer@example.com";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Order.Create(email, price));
    }

    [Fact]
    public void Create_EmailWithMaxLength_ReturnsOrder()
    {
        // Arrange
        var email = CreateEmail(Order.CustomerEmailMaxLength);

        // Act
        var order = Order.Create(email, 1500m);

        // Assert
        Assert.Equal(email, order.CustomerEmail);
    }

    [Fact]
    public void Create_EmailLongerThanMaxLength_ThrowsArgumentException()
    {
        // Arrange
        var email = CreateEmail(Order.CustomerEmailMaxLength + 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => Order.Create(email, 1500m));
    }

    [Fact]
    public void Create_EmailWithSurroundingWhitespace_TrimsBeforeLengthCheck()
    {
        // Arrange
        var email = CreateEmail(Order.CustomerEmailMaxLength);

        // Act
        var order = Order.Create($"  {email}  ", 1500m);

        // Assert
        Assert.Equal(email, order.CustomerEmail);
    }

    [Fact]
    public void Confirm_PendingOrder_ChangesStatusToConfirmed()
    {
        // Arrange
        var order = Order.Create("customer@example.com", 1500m);

        // Act
        order.Confirm();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_ConfirmedOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("customer@example.com", 1500m);
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(order.Confirm);
    }

    [Fact]
    public void Cancel_PendingOrder_ChangesStatusToCancelled()
    {
        // Arrange
        var order = Order.Create("customer@example.com", 1500m);

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_CancelledOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("customer@example.com", 1500m);
        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(order.Cancel);
    }

    [Fact]
    public void Cancel_ConfirmedOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("customer@example.com", 1500m);
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(order.Cancel);
    }

    [Fact]
    public void Confirm_CancelledOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create("customer@example.com", 1500m);
        order.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(order.Confirm);
    }

    private static string CreateEmail(int length)
    {
        const string suffix = "@example.com";
        return new string('a', length - suffix.Length) + suffix;
    }
}
