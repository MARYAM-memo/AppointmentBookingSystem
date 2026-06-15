using AppointmentBooking.Application.ViewModels.Customer;
using AppointmentBooking.Core.Models;

namespace AppointmentBooking.Tests.Helpers;

public class MockDataHelper
{
      /// <summary>
      /// Creates a specified number of mock customers with randomized Arabic names, phone numbers, emails, and appointment history for testing purposes.
      /// </summary>
      public static List<Customer> CreateMockCustomers(int count)
      {
            var customers = new List<Customer>();
            var random = new Random();
            var firstNames = new[] { "أحمد", "محمد", "عبدالله", "فاطمة", "نورة", "سارة", "خالد", "علي", "منى", "هند" };
            var lastNames = new[] { "العمري", "المالكي", "القحطاني", "الشهري", "العتيبي", "الدوسري" };
            var domains = new[] { "gmail.com", "hotmail.com", "outlook.com", "yahoo.com" };

            for (int i = 1; i <= count; i++)
            {
                  var fullName = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
                  var phoneNumber = $"05{random.Next(10000000, 99999999)}";
                  var email = $"{fullName.Replace(" ", ".").ToLower()}@{domains[random.Next(domains.Length)]}";

                  var customer = new Customer
                  {
                        Id = i,
                        FullName = fullName,
                        PhoneNumber = phoneNumber,
                        Email = i % 3 == 0 ? email : null,// Some customers without mail
                        CreatedAt = DateTime.Today.AddDays(-random.Next(0, 365)),
                        LastAppointmentDate = random.Next(0, 100) > 70 ? DateTime.Today.AddDays(-random.Next(0, 30)) : null,
                        TotalAppointments = random.Next(0, 20),
                        Notes = random.Next(0, 100) > 80 ? "عميل مميز" : null
                  };

                  customers.Add(customer);
            }

            return customers;
      }

      /// <summary>
      /// Creates a list of CustomerViewModel from mock customer data for testing ViewModels.
      /// </summary>
      public static List<CustomerViewModel> CreateMockCustomerVMs(int count)
      {
            var customers = CreateMockCustomers(count);
            return customers.Select(c => new CustomerViewModel
            {
                  Id = c.Id,
                  FullName = c.FullName,
                  PhoneNumber = c.PhoneNumber,
                  Email = c.Email,
                  CreatedAt = c.CreatedAt,
                  LastAppointmentDate = c.LastAppointmentDate,
                  TotalAppointments = c.TotalAppointments,
                  Notes = c.Notes
            }).ToList();
      }
}
