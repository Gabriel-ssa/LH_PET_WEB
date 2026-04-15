using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace LH_PET_WEB.Validations
{
    public class CpfAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) 
            return false;

            var cpf = value.ToString()!.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
                return false;

            if (cpf.Distinct().Count() == 1)
                return false;


            int [] multiplicador1 = new int[9] {10, 9, 8, 7, 6, 5, 4, 3, 2};
            int [] multiplicador2 = new int[10] {11, 10, 9, 8, 7, 6, 5, 4, 3, 2};


            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            

            int resto = soma % 11;
            if (resto < 2) resto = 0;
            else resto = 11 - resto;
            
            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2) resto = 0;
            else resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

        public override string FormatErrorMessage(string name)
        {
            return "O CPF informado é inválido.";
        }
    }
}