﻿using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.Models.Orders;
using eShop.ClientApp.Models.User;
using eShop.ClientApp.Services;
using eShop.ClientApp.Services.AppEnvironment;
using eShop.ClientApp.Services.Settings;
using eShop.ClientApp.ViewModels.Base;

namespace eShop.ClientApp.ViewModels;

public partial class CheckoutViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    private readonly IAppEnvironmentService _appEnvironmentService;

    private readonly BasketViewModel _basketViewModel;

    [ObservableProperty]
    private Order _order;

    [ObservableProperty]
    private Address _shippingAddress;

    public CheckoutViewModel(
        BasketViewModel basketViewModel,
        IAppEnvironmentService appEnvironmentService, IDialogService dialogService, ISettingsService settingsService,
        INavigationService navigationService)
        : base(navigationService)
    {
        _dialogService = dialogService;
        _appEnvironmentService = appEnvironmentService;
        _settingsService = settingsService;

        _basketViewModel = basketViewModel;
    }

    public override async Task InitializeAsync()
    {
        await IsBusyFor(
            async () =>
            {
                var basketItems = _appEnvironmentService.BasketService.LocalBasketItems;

                var authToken = _settingsService.AuthAccessToken;
                var userInfo = await _appEnvironmentService.UserService.GetUserInfoAsync(authToken);

                // Create Shipping Address
                ShippingAddress = new Address
                {
                    Id = !string.IsNullOrEmpty(userInfo?.UserId) ? new Guid(userInfo.UserId) : Guid.NewGuid(),
                    Street = userInfo?.Street,
                    ZipCode = userInfo?.ZipCode,
                    State = userInfo?.State,
                    Country = userInfo?.Country,
                    City = userInfo?.Address
                };

                // Create Payment Info
                var paymentInfo = new PaymentInfo
                {
                    CardNumber = userInfo?.CardNumber,
                    CardHolderName = userInfo?.CardHolder,
                    CardType = new CardType { Id = 3, Name = "MasterCard" },
                    SecurityNumber = userInfo?.CardSecurityNumber
                };

                var orderItems = CheckoutViewModel.CreateOrderItems(basketItems);

                // Create new Order
                Order = new Order
                {
                    BuyerId = userInfo.UserId,
                    OrderItems = orderItems,
                    OrderStatus = OrderStatus.Submitted,
                    OrderDate = DateTime.Now,
                    CardHolderName = paymentInfo.CardHolderName,
                    CardNumber = paymentInfo.CardNumber,
                    CardSecurityNumber = paymentInfo.SecurityNumber,
                    CardExpiration = DateTime.Now.AddYears(5),
                    CardTypeId = paymentInfo.CardType.Id,
                    ShippingState = ShippingAddress.State,
                    ShippingCountry = ShippingAddress.Country,
                    ShippingStreet = ShippingAddress.Street,
                    ShippingCity = ShippingAddress.City,
                    ShippingZipCode = ShippingAddress.ZipCode,
                    Total = CheckoutViewModel.CalculateTotal(orderItems),
                };

                if (_settingsService.UseMocks)
                {
                    // Get number of orders
                    var orders = await _appEnvironmentService.OrderService.GetOrdersAsync(authToken);

                    // Create the OrderNumber
                    Order.OrderNumber = orders.Count() + 1;
                    OnPropertyChanged(nameof(Order));
                }
            });
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        try
        {
            var authToken = _settingsService.AuthAccessToken;

            var basket = _appEnvironmentService.OrderService.MapOrderToBasket(Order);
            basket.RequestId = Guid.NewGuid();

            // Create basket checkout
            await _appEnvironmentService.BasketService.CheckoutAsync(basket, authToken);

            if (_settingsService.UseMocks)
            {
                await _appEnvironmentService.OrderService.CreateOrderAsync(Order, authToken);
            }

            // Clean Basket
            await _appEnvironmentService.BasketService.ClearBasketAsync(ShippingAddress.Id.ToString(), authToken);

            // Reset Basket badge
            _basketViewModel.ClearBasketItems();

            // Navigate to Orders
            await NavigationService.NavigateToAsync("//Main/Catalog");

            // Show Dialog
            await _dialogService.ShowAlertAsync("Order sent successfully!", "Checkout", "Ok");
        }
        catch
        {
            await _dialogService.ShowAlertAsync("An error ocurred. Please, try again.", "Oops!", "Ok");
        }
    }

    private static List<OrderItem> CreateOrderItems(IEnumerable<BasketItem> basketItems)
    {
        var orderItems = new List<OrderItem>();

        foreach (var basketItem in basketItems)
        {
            if (!string.IsNullOrEmpty(basketItem.ProductName))
            {
                orderItems.Add(new OrderItem
                {
                    OrderId = null,
                    ProductId = basketItem.ProductId,
                    ProductName = basketItem.ProductName,
                    PictureUrl = basketItem.PictureUrl,
                    Quantity = basketItem.Quantity,
                    UnitPrice = basketItem.UnitPrice
                });
            }
        }

        return orderItems;
    }

    private static decimal CalculateTotal(List<OrderItem> orderItems)
    {
        decimal total = 0;

        foreach (var orderItem in orderItems)
        {
            total += (orderItem.Quantity * orderItem.UnitPrice);
        }

        return total;
    }
}
