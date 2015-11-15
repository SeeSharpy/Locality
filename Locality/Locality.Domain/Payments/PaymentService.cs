﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Localisty.Data.Entities.Tickets;
using Locality.Data.Entities.Events;
using Locality.Data.Entities.Users;
using SimplifyCommerce.Payments;

namespace Locality.Domain.Payments
{
    public class PaymentService : IPaymentService
    {
        public Task<Localisty.Data.Entities.Tickets.Tickets> BuyTicketWithToken(string token, Data.Entities.Events.Events buyEvent, User userBuying)
        {
            PaymentsApi.PublicApiKey = "sbpb_MzlkM2M1MjItZDkwZi00NTY5LWI1YzAtOGFiM2Y5YWJlMzQx";
            PaymentsApi.PrivateApiKey = Environment.GetEnvironmentVariable("SimplifyPrivate");

            var api = new PaymentsApi();


            var customer = new Customer
            {
                Token = token,
                Name = userBuying.Name,
                Reference = "Locality Customer",
                Email = "test@email.com"
            };

            try
            {

                customer = (Customer) api.Create(customer);

                var payment = new Payment
                {
                    Amount = (long)buyEvent.Price * 100,
                    Currency = "GBP",
                    Description = $"Ticket for {buyEvent.Name}",
                    Customer = customer
                };


                payment = (Payment)api.Create(payment);

                    userBuying.CustomerId = customer.Id;
                    return Task.FromResult(new Localisty.Data.Entities.Tickets.Tickets
                    {
                        Id = Guid.NewGuid(),
                        Barcode = payment.Reference,
                        EventId = buyEvent.Id,
                        UserId = userBuying.Id,
                        Used = false
                    });

            }
            catch
            {
                return null;
            }
        }

        public Task<Localisty.Data.Entities.Tickets.Tickets> BuyTicketWithCustomer(string customerId, Data.Entities.Events.Events buyEvent, User userBuying)
        {
            PaymentsApi.PublicApiKey = "sbpb_MzlkM2M1MjItZDkwZi00NTY5LWI1YzAtOGFiM2Y5YWJlMzQx";
            PaymentsApi.PrivateApiKey = Environment.GetEnvironmentVariable("SimplifyPrivate");

            var api = new PaymentsApi();

            try
            {
                var customer = (Customer) api.Find(typeof (Customer), customerId);

                if (customer == null)
                {
                    return null;
                }
                var payment = new Payment
                {
                    Amount = (long)buyEvent.Price,
                    Currency = "GBP",
                    Description = $"Ticket for {buyEvent.Name}",
                    Customer = customer
                };

                payment = (Payment)api.Create(payment);

                if (payment.Authorization.Id != null)
                {
                    return Task.FromResult(new Localisty.Data.Entities.Tickets.Tickets
                    {
                        Barcode = payment.Reference,
                        Event = buyEvent,
                        User = userBuying,
                        Used = false
                    });
                }

                return null;

            }
            catch
            {
                return null;
            }
        }
    }
}
