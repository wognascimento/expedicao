using System.Collections.Generic;
using System.Globalization;
using Telerik.Windows.Controls;

namespace Expedicao.Localization
{
    internal sealed class SigTelerikLocalizationManager : LocalizationManager
    {
        private static readonly Dictionary<string, string> DockingResources = new()
        {
            ["Hide"] = "Ocultar",
            ["Auto_hide"] = "Ocultar automaticamente",
            ["Close"] = "Fechar",
            ["CloseItem"] = "Fechar",
            ["Dockable"] = "Encaixavel",
            ["Docking_ActiveDocuments"] = "Documentos ativos",
            ["Docking_ActivePanes"] = "Paineis ativos",
            ["Docking_HideActivePane"] = "Ocultar painel ativo",
            ["Docking_PreviewHeader"] = "Visualizacao",
            ["Floating"] = "Flutuante",
            ["Pin"] = "Fixar",
            ["Tabbed_document"] = "Documento em aba",
        };

        public override string GetStringOverride(string key)
        {
            var resourceValue = GridViewResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
            if (!string.IsNullOrWhiteSpace(resourceValue))
            {
                return resourceValue;
            }

            return DockingResources.TryGetValue(key, out var dockingValue)
                ? dockingValue
                : base.GetStringOverride(key);
        }
    }
}
