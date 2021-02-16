using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App
{
    public class Account
    {
        private INotificationService notificationService;

        public Account(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        public const decimal PayInLimit = 4000m;
        public const decimal LowFundsLimit = 500m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        public void Withdraw(decimal amount)
        {
            if(amount < 0m)
            {
                throw new ArgumentException("Negative withdrawals are not allowed");
            }

            var fromBalance = Balance - amount;
            if (fromBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

            if (fromBalance < LowFundsLimit)
            {
                this.notificationService.NotifyFundsLow(User.Email);
            }

            Balance -= amount;
            Withdrawn -= amount;
        }

        public void PayIn(decimal amount)
        {
            if (amount < 0m)
            {
                throw new ArgumentException("Negative pay ins are not allowed");
            }

            var paidIn = PaidIn + amount;
            if (paidIn > Account.PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            if (Account.PayInLimit - paidIn < LowFundsLimit)
            {
                this.notificationService.NotifyApproachingPayInLimit(User.Email);
            }

            Balance += amount;
            PaidIn += amount;
        }
    }
}
