using API.Models;
using API.Models.DTO;
using BankAccountSimulation.UI.Models;
using BankAccountSimulation.UI.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using OfficeOpenXml;
using System;
using static BankAccountSimulation.UI.Helper.Helper;


namespace BankAccountSimulation.UI.Controllers
{
    public class BankController : Controller
    {
        private readonly IAccountApiService _accountApiService;
        private const int PageSize = 5;

        string accountNo = "4A62AE47-3CEE-4F69-0CBC-08DCF0F3B2D2";//Selva-test Only

        public BankController(IAccountApiService accountApiService)
        {
            _accountApiService = accountApiService;
        }

        // GET: /Accounts/List
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            try
            {
                var allAccounts = await _accountApiService.GetAllAccountsAsync();
                var paginatedAccounts = allAccounts
                    .Skip((pageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                var model = new PaginationModel<AccountDTO>
                {
                    Items = paginatedAccounts,
                    PageNumber = pageNumber,
                    TotalPages = (int)Math.Ceiling(allAccounts.Count() / (double)PageSize)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the account: " + ex.Message;
                return View("Error");
            }
        }

        // GET: /Accounts/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountDTO accountDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(accountDTO);
            }

            try
            {
                var accountId = await _accountApiService.CreateAccountAsync(accountDTO);
                return RedirectToAction(nameof(Index), new { id = accountId });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the account: " + ex.Message;
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var account = await _accountApiService.GetAccountWithTransactionsAsync(id);

                // Prepare data for the chart
                var depositSum = account.Transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount);
                var withdrawalSum = account.Transactions.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount);
                var currentBalance = account.Balance;

                ViewBag.TransactionSummary = new[] { depositSum, withdrawalSum, currentBalance };

                return View(account);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving the account details.";
                return View("Error");
            }
        }


        // GET: /Accounts/DepositWithdraw
        [HttpGet]
        public IActionResult DepositWithdraw(Guid id)
        {
            DepositWithdrawDTO depositWithdraw = new DepositWithdrawDTO();
            depositWithdraw.AccountId = id;
            return View(depositWithdraw);
        }

        [HttpPost]
        public async Task<IActionResult> DepositWithdraw(DepositWithdrawDTO model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Type == TransactionType.Deposit)
                    {
                        await _accountApiService.DepositAsync(model.AccountId, model.Amount, model.Description);
                        return RedirectToAction(nameof(Index), new { id = model.AccountId });
                    }
                    else if (model.Type == TransactionType.Withdrawal)
                    {
                        await _accountApiService.WithdrawAsync(model.AccountId, model.Amount, model.Description);
                    }

                    return RedirectToAction("Index", new { id = model.AccountId });
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An error occurred while processing the transaction.";
                    return View("Error");
                }
            }
            return View(model);
        }

        // GET: /Accounts/Transactions/5
        public async Task<IActionResult> Transactions(Guid id)
        {
            try
            {
                var accountWithTransactions = await _accountApiService.GetAccountWithTransactionsAsync(id);
                return View(accountWithTransactions);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving the account transactions.";
                return View("Error");
            }
        }

        public async Task<IActionResult> ExportToExcel(Guid accountId)
        {
            var account = await _accountApiService.GetAccountWithTransactionsAsync(accountId);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Account Details and Transactions");

                // Set account details
                worksheet.Cells[1, 1].Value = "Account Details";
                worksheet.Cells[1, 1, 1, 4].Merge = true; // Merge cells for title
                worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                worksheet.Cells[1, 1, 1, 4].Style.Font.Color.SetColor(System.Drawing.Color.White);

                worksheet.Cells[2, 1].Value = "Full Name";
                worksheet.Cells[2, 2].Value = account.FullName;
                worksheet.Cells[3, 1].Value = "Account Number";
                worksheet.Cells[3, 2].Value = account.AccountNumber;
                worksheet.Cells[4, 1].Value = "Balance";
                worksheet.Cells[4, 2].Value = account.Balance.ToString("C");

                // Add a header for transactions section
                worksheet.Cells[6, 1].Value = "Transactions";
                worksheet.Cells[6, 1, 6, 4].Merge = true; // Merge cells for title
                worksheet.Cells[6, 1, 6, 4].Style.Font.Bold = true;
                worksheet.Cells[6, 1, 6, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[6, 1, 6, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                worksheet.Cells[6, 1, 6, 4].Style.Font.Color.SetColor(System.Drawing.Color.White);

                // Set headers for transactions
                worksheet.Cells[7, 1].Value = "Transaction Date";
                worksheet.Cells[7, 2].Value = "Type";
                worksheet.Cells[7, 3].Value = "Amount";
                worksheet.Cells[7, 4].Value = "Description";

                // Header styling
                using (var range = worksheet.Cells[7, 1, 7, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                // Populate data
                var row = 8;
                foreach (var transaction in account.Transactions)
                {
                    worksheet.Cells[row, 1].Value = transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cells[row, 2].Value = transaction.Type.ToString();
                    worksheet.Cells[row, 3].Value = transaction.Amount;
                    worksheet.Cells[row, 4].Value = transaction.Description;

                    // Row styling
                    if (row % 2 == 0)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 4])
                        {
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        }
                    }
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var fileName = $"Account_{account.AccountNumber}_Details_And_Transactions.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }


        public async Task<IActionResult> ExportToPdf(Guid accountId)
        {
            var account = await _accountApiService.GetAccountWithTransactionsAsync(accountId);

            var stream = new MemoryStream();
            var document = new Document();
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // Set font styles
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLUE);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

            // Add account details header
            var accountDetailsHeader = new Paragraph("Account Details", titleFont);
            accountDetailsHeader.Alignment = Element.ALIGN_CENTER;
            document.Add(accountDetailsHeader);
            document.Add(new Paragraph(" ")); // Add some space

            // Add account details
            var accountDetailsTable = new PdfPTable(2) { WidthPercentage = 100 };
            accountDetailsTable.AddCell(new PdfPCell(new Phrase("Full Name:", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
            accountDetailsTable.AddCell(new Phrase(account.FullName, normalFont));
            accountDetailsTable.AddCell(new PdfPCell(new Phrase("Account Number:", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
            accountDetailsTable.AddCell(new Phrase(account.AccountNumber, normalFont));
            accountDetailsTable.AddCell(new PdfPCell(new Phrase("Balance:", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
            accountDetailsTable.AddCell(new Phrase(account.Balance.ToString("C"), normalFont));

            document.Add(accountDetailsTable);
            document.Add(new Paragraph(" ")); // Add some space

            // Add transactions header
            var transactionsHeader = new Paragraph("Transactions", titleFont);
            transactionsHeader.Alignment = Element.ALIGN_CENTER;
            document.Add(transactionsHeader);
            document.Add(new Paragraph(" ")); // Add some space

            // Add transactions table
            var table = new PdfPTable(4) { WidthPercentage = 100 };
            table.AddCell(new PdfPCell(new Phrase("Transaction Date", headerFont)) { BackgroundColor = BaseColor.DARK_GRAY });
            table.AddCell(new PdfPCell(new Phrase("Type", headerFont)) { BackgroundColor = BaseColor.DARK_GRAY });
            table.AddCell(new PdfPCell(new Phrase("Amount", headerFont)) { BackgroundColor = BaseColor.DARK_GRAY });
            table.AddCell(new PdfPCell(new Phrase("Description", headerFont)) { BackgroundColor = BaseColor.DARK_GRAY });

            foreach (var transaction in account.Transactions)
            {
                table.AddCell(new Phrase(transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss"), normalFont));
                table.AddCell(new Phrase(transaction.Type.ToString(), normalFont));
                table.AddCell(new Phrase(transaction.Amount.ToString("C"), normalFont));
                table.AddCell(new Phrase(transaction.Description, normalFont));
            }

            document.Add(table);
            document.Close();

            stream.Position = 0;
            var fileName = $"Account_{account.AccountNumber}_Transactions.pdf";
            return File(stream, "application/pdf", fileName);
        }


        public async Task<IActionResult> Dashboard(Guid accountId)
        {
            try
            {
                //string accountNo = "684077DA-C896-4EF0-39F7-08DCEFE41206";//Selva-test

                bool isValid = Guid.TryParse(accountNo, out accountId);
               
                var account = await _accountApiService.GetAccountWithTransactionsAsync(accountId);

                var model = new DashboardViewModel
                {
                    CurrentBalance = account.Balance,
                    TotalDeposits = account.Transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount),
                    TotalExpenses = account.Transactions.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "An error occurred while retrieving dashboard data: " + ex.Message;
                return View();
            }
        }


        public async Task<IActionResult> Deposits(Guid accountId)
        {
            try
            {
                //string accountNo = "684077DA-C896-4EF0-39F7-08DCEFE41206";//Selva-test
                bool isValid = Guid.TryParse(accountNo, out accountId);
                var deposits = await _accountApiService.GetDepositsAsync(accountId); 
                var totalDeposits = deposits.Sum(d => d.Amount);

                var model = new DepositsViewModel
                {
                    Deposits = deposits,
                    TotalDeposits = totalDeposits
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = AlertType.danger.ToString();
                TempData["AlertMessage"] = "An error occurred while retrieving deposits: " + ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> Expenses(Guid accountId)
        {
            try
            {
                //string accountNo = "684077DA-C896-4EF0-39F7-08DCEFE41206";//Selva-test
                bool isValid = Guid.TryParse(accountNo, out accountId);
                var expenses = await _accountApiService.GetExpensesAsync(accountId); 
                var totalExpenses = expenses.Sum(e => e.Amount);

                var model = new ExpensesViewModel
                {
                    Expenses = expenses,
                    TotalExpenses = totalExpenses
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = AlertType.danger.ToString();
                TempData["AlertMessage"] = "An error occurred while retrieving expenses: " + ex.Message;
                return View();
            }
        }


        // GET: /Accounts/InterestRules
        [HttpGet]
        public IActionResult InterestRules()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InterestRules(InterestDTO interestDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(interestDTO);
            }

            try
            {
                var interestId = await _accountApiService.CreateInterestAsync(interestDTO);
                return RedirectToAction(nameof(InterestList), new { id = interestId });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the interest rules: " + ex.Message;
                return View("Error");
            }
        }

        // GET: /Accounts/List
        public async Task<IActionResult> InterestList(int pageNumber = 1)
        {
            try
            {
                var allInterests = await _accountApiService.GetAllInterestsAsync();
                var paginatedInterests = allInterests
                    .Skip((pageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                var model = new PaginationModel<InterestDTO>
                {
                    Items = paginatedInterests,
                    PageNumber = pageNumber,
                    TotalPages = (int)Math.Ceiling(allInterests.Count() / (double)PageSize)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the Interest rates: " + ex.Message;
                return View("Error");
            }
        }

    }
}
