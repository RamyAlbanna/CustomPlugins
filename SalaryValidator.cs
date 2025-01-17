using Microsoft.Xrm.Sdk;
using System;

public class SalaryValidator : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity targetEntity)
        {
            if (targetEntity.LogicalName == "contact" && targetEntity.Attributes.Contains("ram_salary"))
            {
                Money newSalaryMoney = (Money)targetEntity["ram_salary"];
                decimal newSalaryValue = newSalaryMoney.Value;
                Money oldSalaryMoney = (Money)context.PreEntityImages["ram"]["ram_salary"];
                decimal oldSalaryValue = oldSalaryMoney.Value;
                if (Math.Abs(newSalaryValue - oldSalaryValue) > 3000)
                {
                    throw new InvalidPluginExecutionException("Salary change must not exceed ±3000.");
                }
            }
        }
    }
}