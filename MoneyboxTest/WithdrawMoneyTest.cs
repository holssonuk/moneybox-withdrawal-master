using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneybox.App;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using System;

namespace MoneyboxTest
{
    [TestClass]
    public class WithdrawMoneyTest
    {
        private Mock<IAccountRepository> mockAccountRepository;
        private Mock<INotificationService> mockNotificationService;
        private WithdrawMoney withdrawMoney;
        private Guid fromAccountId;

        [TestInitialize]
        public void Init()
        {
            mockAccountRepository = new Mock<IAccountRepository>();
            mockNotificationService = new Mock<INotificationService>();
            fromAccountId = Guid.NewGuid();
        }

        [TestMethod]
        public void Execute_Successful()
        {
            const decimal amount = 200m;

            var accountRepository = mockAccountRepository.Object;
            var notificationService = mockNotificationService.Object;

            var account = new Account(notificationService)
            {
                Id = fromAccountId,
                Balance = 1000m
            };

            mockAccountRepository.Setup(x => x.GetAccountById(It.IsAny<Guid>()))
                .Returns(account);
            mockAccountRepository.Setup(x => x.Update(It.IsAny<Account>()));

            withdrawMoney = new WithdrawMoney(accountRepository);

            withdrawMoney.Execute(fromAccountId, amount);

            mockAccountRepository.Verify(x => x.GetAccountById(fromAccountId));

            mockAccountRepository.Verify(x => x.Update(account));

            Assert.AreEqual(800m, account.Balance);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_InsufficientFunds()
        {
            const decimal amount = 1200m;

            var accountRepository = mockAccountRepository.Object;
            var notificationService = mockNotificationService.Object;

            var account = new Account(notificationService)
            {
                Id = fromAccountId,
                Balance = 1000m
            };

            mockAccountRepository.Setup(x => x.GetAccountById(It.IsAny<Guid>()))
                .Returns(account);
            withdrawMoney = new WithdrawMoney(accountRepository);

            withdrawMoney.Execute(fromAccountId, amount);

            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Execute_NegativeWithdrawal()
        {
            const decimal amount = -100;

            var accountRepository = mockAccountRepository.Object;
            var notificationService = mockNotificationService.Object;

            var account = new Account(notificationService)
            {
                Id = fromAccountId,
                Balance = 1000m
            };

            mockAccountRepository.Setup(x => x.GetAccountById(It.IsAny<Guid>()))
                .Returns(account);
            withdrawMoney = new WithdrawMoney(accountRepository);

            withdrawMoney.Execute(fromAccountId, amount);
        }

        [TestMethod]
        public void Execute_FundsLow()
        {
            const decimal amount = 800m;
            const string emailAddress = "test@test.com";

            var accountRepository = mockAccountRepository.Object;
            var notificationService = mockNotificationService.Object;

            var account = new Account(notificationService)
            {
                Id = fromAccountId,
                Balance = 1000m,
                User = new User
                {
                    Email = emailAddress
                }
            };

            mockAccountRepository.Setup(x => x.GetAccountById(It.IsAny<Guid>()))
                .Returns(account);
            mockAccountRepository.Setup(x => x.Update(It.IsAny<Account>()));

            mockNotificationService.Setup(x => x.NotifyFundsLow(It.IsAny<string>()));

            withdrawMoney = new WithdrawMoney(accountRepository);

            withdrawMoney.Execute(fromAccountId, amount);

            mockAccountRepository.Verify(x => x.GetAccountById(fromAccountId));

            mockAccountRepository.Verify(x => x.Update(account));

            mockNotificationService.Verify(x => x.NotifyFundsLow(account.User.Email));

            Assert.AreEqual(200m, account.Balance);
        }
    }
}
