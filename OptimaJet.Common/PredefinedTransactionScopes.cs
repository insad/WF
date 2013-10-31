using System;
using System.Transactions;

namespace OptimaJet.Common
{
    public static class PredefinedTransactionScopes
    {
        public static TransactionScope SerializableSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions
                                                {
                                                    IsolationLevel = IsolationLevel.Serializable,
                                                    Timeout = new TimeSpan(0, 10, 0)
                                                });
            }
        }

        public static TransactionScope ReadCommittedSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions
                                                {
                                                    IsolationLevel = IsolationLevel.ReadCommitted,
                                                    Timeout = new TimeSpan(0, 10, 0)
                                                });
            }
        }

        public static TransactionScope ReadUncommittedSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions
                                                {
                                                    IsolationLevel = IsolationLevel.ReadUncommitted,
                                                    Timeout = new TimeSpan(0, 10, 0)
                                                });
            }
        }
    }
}