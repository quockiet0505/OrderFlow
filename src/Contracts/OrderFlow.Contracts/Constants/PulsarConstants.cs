namespace OrderFlow.Contracts.Constants;

public static class PulsarTopics
{
    public const string OrderPlaced = "persistent://public/default/orders.order-placed";
    public const string ReservationEvents = "persistent://public/default/reservation-events";
    public const string PaymentEvents = "persistent://public/default/payment-events";
}

public static class PulsarSubscriptions
{
    public const string OrdersReservation = "orders-saga-sub";
    public const string OrdersPayment = "orders-payment-sub";
    public const string InventoryOrder = "inventory-order-sub";
    public const string InventoryPayment = "inventory-payment-sub";
    public const string PaymentsReservation = "payments-reservation-sub";
}
