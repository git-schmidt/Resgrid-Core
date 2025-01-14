﻿using System.Threading;
using System.Threading.Tasks;
using Resgrid.Model;
using Resgrid.Model.Services;
using Stripe;
using Stripe.Checkout;

namespace Resgrid.Services
{
	public class PaymentProviderService : IPaymentProviderService
	{
		#region Private Methods and Constructors

		public PaymentProviderService()
		{

		}
		#endregion Private Methods and Constructors

		public async Task<PaymentProviderEvent> SaveEventAsync(PaymentProviderEvent providerEvent,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		public async Task<Payment> ProcessStripePaymentAsync(Charge charge)
		{
			return null;
		}

		public async Task<Payment> ProcessStripeSubscriptionUpdateAsync(Subscription stripeSubscription,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		public async Task<Payment> ProcessStripeSubscriptionCancellationAsync(Subscription stripeSubscription,
			CancellationToken cancellationToken = default(CancellationToken))
		{		
			return null;
		}

		public async Task<Payment> ProcessStripeSubscriptionRefundAsync(Charge stripeCharge)
		{
			return null;
		}

		public async Task<Payment> ProcessStripeChargeFailedAsync(Charge stripeCharge)
		{
			return null;
		}

		public async Task<Session> CreateStripeSessionForSub(int departmentId, string stripeCustomerId, string stripePlanId, int planId, string emailAddress, string departmentName)
		{
			return null;
		}

		public async Task<Session> CreateStripeSessionForUpdate(int departmentId, string stripeCustomerId, string emailAddress, string departmentName)
		{
			return null;
		}

		public async Task<Payment> ProcessStripeCheckoutCompletedAsync(Session session,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		public void ProcessStripeCheckoutUpdate(Session session)
		{
			return;
		}

		public Subscription GetActiveStripeSubscription(string stripeCustomerId)
		{
			return null;
		}

		public Invoice ChangeActiveSubscription(string stripeCustomerId, string stripePlanId)
		{
			return null;
		}

		public async Task<Customer> ProcessStripeCheckoutUpdateAsync(Session session,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		public async Task<Payment> ProcessStripeInvoicePaidAsync(Invoice invoice)
		{
			return null;
		}
	}
}
