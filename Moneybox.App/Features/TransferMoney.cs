using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;

        public TransferMoney(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = this.accountRepository.GetAccountById(fromAccountId);
            var to = this.accountRepository.GetAccountById(toAccountId);

            from.Withdraw(amount);
            to.PayIn(amount);

            // I'd really want to have a transaction around these two
            this.accountRepository.Update(from);
            this.accountRepository.Update(to);
        }
    }
}
