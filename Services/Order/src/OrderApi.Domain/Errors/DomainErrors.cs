using OrderApi.Domain.Primitives;

namespace OrderApi.Domain.Errors;

public static class DomainErrors
{
    public static class Order
    {
        // Validation: żądanie jest złe – po poprawieniu danych przejdzie.
        public static readonly Error Empty =
            Error.Validation("Order.Empty", "Order must contain at least one line.");

        public static readonly Error CurrencyMismatch =
            Error.Validation("Order.CurrencyMismatch", "All order lines must use the same currency.");

        // Conflict: dane są poprawne, ale obecny stan zamówienia na to nie pozwala.
        public static readonly Error DuplicateProduct =
            Error.Conflict("Order.DuplicateProduct", "This product is already in the order.");

        public static readonly Error NotPending =
            Error.Conflict("Order.NotPending", "Lines can only be added to a pending order.");

        public static readonly Error CannotPay =
            Error.Conflict("Order.CannotPay", "Only pending orders can be paid.");

        public static readonly Error CannotShip =
            Error.Conflict("Order.CannotShip", "Only paid orders can be shipped.");

        public static readonly Error CannotDeliver =
            Error.Conflict("Order.CannotDeliver", "Only shipped orders can be delivered.");

        public static readonly Error CannotCancel =
            Error.Conflict("Order.CannotCancel", "Only pending or paid orders can be cancelled.");
    }
}
