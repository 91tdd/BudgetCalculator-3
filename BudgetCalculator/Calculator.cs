using System;
using System.Linq;

namespace BudgetCalculator
{
    public class Calculator
    {
        private IBudgetRepository _budgetRepository;

        public void SetData(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }

        public decimal CalculateBudget(DateTime start, DateTime end)
        {
            var budgets = _budgetRepository.GetAll();

            var budget = 0M;

            foreach (var budgetModel in budgets)
            {
                if (budgetModel.YearMonth != start.ToString("yyyyMM") &&
                    budgetModel.YearMonth != end.ToString("yyyyMM"))
                {
                    DateTime firstDayOfBudgetMonth = DateTime.ParseExact(budgetModel.YearMonth + "01", "yyyyMMdd", null);
                    if (firstDayOfBudgetMonth > start && firstDayOfBudgetMonth < end)
                    {
                        budget += budgetModel.Budget;
                    }
                }
            }

            if (start.ToString("yyyyMM") != end.ToString("yyyyMM"))
            {
                var totalStartDaysInAMonth = DateTime.DaysInMonth(start.Year, start.Month);
                var startDays = CalculateDays(start, new DateTime(start.Year, start.Month, totalStartDaysInAMonth));
                var startBudgetModels = budgets.Where(model => { return model.YearMonth == start.ToString("yyyyMM"); });
                if (startBudgetModels.Any())
                {
                    budget += startBudgetModels.First().Budget / totalStartDaysInAMonth * startDays;
                }

                var endDays = CalculateDays(new DateTime(end.Year, end.Month, 1), end);
                var totalEndDaysInAMonth = DateTime.DaysInMonth(end.Year, end.Month);
                var endBudgetModels = budgets.Where(model => { return model.YearMonth == end.ToString("yyyyMM"); });
                if (endBudgetModels.Any())
                {
                    budget += endBudgetModels.First().Budget / totalEndDaysInAMonth * endDays;
                }

                return budget;
            }

            var days = CalculateDays(start, end);
            var totalDaysInAMonth = DateTime.DaysInMonth(start.Year, start.Month);
            var budgetModels = budgets.Where(model => { return model.YearMonth == start.ToString("yyyyMM"); });
            if (!budgetModels.Any())
            {
                return 0;
            }

            return budgetModels.First().Budget / totalDaysInAMonth * days;
        }

        private int CalculateDays(DateTime start, DateTime end)
        {
            return (end - start).Days + 1;
        }
    }
}