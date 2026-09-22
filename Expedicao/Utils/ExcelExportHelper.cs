using ClosedXML.Excel;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Expedicao.Utils;

internal static class ExcelExportHelper
{
    public static int WritePlainData(
        IXLWorksheet worksheet,
        IEnumerable dados,
        int linhaInicial = 1,
        bool incluirCabecalho = true)
    {
        var linhas = dados.Cast<object>().ToList();
        if (linhas.Count == 0)
        {
            return 0;
        }

        var propriedades = linhas[0].GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
            .ToArray();

        var linha = linhaInicial;
        if (incluirCabecalho)
        {
            for (var coluna = 0; coluna < propriedades.Length; coluna++)
            {
                worksheet.Cell(linha, coluna + 1).Value = propriedades[coluna].Name;
            }

            linha++;
        }

        foreach (var item in linhas)
        {
            for (var coluna = 0; coluna < propriedades.Length; coluna++)
            {
                var valor = propriedades[coluna].GetValue(item);
                worksheet.Cell(linha, coluna + 1).Value = valor is null
                    ? string.Empty
                    : XLCellValue.FromObject(valor);
            }

            linha++;
        }

        return linhas.Count;
    }
}
