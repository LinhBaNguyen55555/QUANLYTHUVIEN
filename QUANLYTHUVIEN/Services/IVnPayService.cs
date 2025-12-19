using Microsoft.AspNetCore.Http;
using QUANLYTHUVIEN.Models;

namespace QUANLYTHUVIEN.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}










