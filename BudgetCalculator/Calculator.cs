using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetCalculator
{
    public class Calculator
    {
        private IBudgetRepository _budgetRepository;

        public Calculator(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }

        public decimal CalculateBudget(DateTime start, DateTime end)
        {
            var period = new Period(start, end);

            var budgets = _budgetRepository.GetAll();

            var totalAmount = 0M;

            foreach (var budgetModel in budgets)
            {
                if (IsMiddleMonth(period, budgetModel))
                {
                    totalAmount += budgetModel.Amount;
                }
            }

            if (period.IsCrossMonth())
            {
                var amountOfFirstMonth = AmountOfFirstMonth(period, budgets);
                totalAmount += amountOfFirstMonth;

                var endDays = CalculateDays(new DateTime(period.End.Year, period.End.Month, 1), period.End);
                var totalEndDaysInAMonth = DateTime.DaysInMonth(period.End.Year, period.End.Month);
                var endBudgetModels = budgets.Where(model => { return model.YearMonth == period.End.ToString("yyyyMM"); });
                if (endBudgetModels.Any())
                {
                    totalAmount += endBudgetModels.First().Amount / totalEndDaysInAMonth * endDays;
                }

                return totalAmount;
            }

            var days = CalculateDays(period.Start, period.End);
            var totalDaysInAMonth = DateTime.DaysInMonth(period.Start.Year, period.Start.Month);
            var budgetModels = budgets.Where(model => { return model.YearMonth == period.Start.ToString("yyyyMM"); });
            if (!budgetModels.Any())
            {
                return 0;
            }

            return budgetModels.First().Amount / totalDaysInAMonth * days;
        }

        private decimal AmountOfFirstMonth(Period period, List<BudgetModel> budgets)
        {
            var amountOfFirstMonth = 0m;
            var totalStartDaysInAMonth = DateTime.DaysInMonth(period.Start.Year, period.Start.Month);
            var startDays = CalculateDays(period.Start,
                new DateTime(period.Start.Year, period.Start.Month, totalStartDaysInAMonth));
            var startBudgetModels = budgets.Where(model => { return model.YearMonth == period.Start.ToString("yyyyMM"); });

            if (startBudgetModels.Any())
            {
                amountOfFirstMonth = startBudgetModels.First().Amount / totalStartDaysInAMonth * startDays;
            }

            return amountOfFirstMonth;
        }

        private static bool IsMiddleMonth(Period period, BudgetModel budgetModel)
        {
            return budgetModel.YearMonth != period.Start.ToString("yyyyMM") &&
                   budgetModel.YearMonth != period.End.ToString("yyyyMM");
        }

        private int CalculateDays(DateTime start, DateTime end)
        {
            return (end - start).Days + 1;
        }
    }
}