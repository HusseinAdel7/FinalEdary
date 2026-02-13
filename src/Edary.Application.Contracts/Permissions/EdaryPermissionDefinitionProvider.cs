using Edary.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Edary.Permissions;

public class EdaryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var edaryGroup = context.AddGroup(EdaryPermissions.GroupName, L("Permission:Edary"));

        // Items
        var itemsPermission = edaryGroup.AddPermission(
            EdaryPermissions.Items.Default,
            L("Permission:Items"));
        itemsPermission.AddChild(
            EdaryPermissions.Items.List,
            L("Permission:Items.List"));
        itemsPermission.AddChild(
            EdaryPermissions.Items.Create,
            L("Permission:Items.Create"));
        itemsPermission.AddChild(
            EdaryPermissions.Items.Update,
            L("Permission:Items.Update"));
        itemsPermission.AddChild(
            EdaryPermissions.Items.Delete,
            L("Permission:Items.Delete"));

        // Warehouses
        var warehousesPermission = edaryGroup.AddPermission(
            EdaryPermissions.Warehouses.Default,
            L("Permission:Warehouses"));
        warehousesPermission.AddChild(
            EdaryPermissions.Warehouses.List,
            L("Permission:Warehouses.List"));
        warehousesPermission.AddChild(
            EdaryPermissions.Warehouses.Create,
            L("Permission:Warehouses.Create"));
        warehousesPermission.AddChild(
            EdaryPermissions.Warehouses.Update,
            L("Permission:Warehouses.Update"));
        warehousesPermission.AddChild(
            EdaryPermissions.Warehouses.Delete,
            L("Permission:Warehouses.Delete"));

        // Suppliers
        var suppliersPermission = edaryGroup.AddPermission(
            EdaryPermissions.Suppliers.Default,
            L("Permission:Suppliers"));
        suppliersPermission.AddChild(
            EdaryPermissions.Suppliers.List,
            L("Permission:Suppliers.List"));
        suppliersPermission.AddChild(
            EdaryPermissions.Suppliers.Create,
            L("Permission:Suppliers.Create"));
        suppliersPermission.AddChild(
            EdaryPermissions.Suppliers.Update,
            L("Permission:Suppliers.Update"));
        suppliersPermission.AddChild(
            EdaryPermissions.Suppliers.Delete,
            L("Permission:Suppliers.Delete"));

        // MainAccounts
        var mainAccountsPermission = edaryGroup.AddPermission(
            EdaryPermissions.MainAccounts.Default,
            L("Permission:MainAccounts"));
        mainAccountsPermission.AddChild(
            EdaryPermissions.MainAccounts.List,
            L("Permission:MainAccounts.List"));
        mainAccountsPermission.AddChild(
            EdaryPermissions.MainAccounts.Create,
            L("Permission:MainAccounts.Create"));
        mainAccountsPermission.AddChild(
            EdaryPermissions.MainAccounts.Update,
            L("Permission:MainAccounts.Update"));
        mainAccountsPermission.AddChild(
            EdaryPermissions.MainAccounts.Delete,
            L("Permission:MainAccounts.Delete"));

        // SubAccounts
        var subAccountsPermission = edaryGroup.AddPermission(
            EdaryPermissions.SubAccounts.Default,
            L("Permission:SubAccounts"));
        subAccountsPermission.AddChild(
            EdaryPermissions.SubAccounts.List,
            L("Permission:SubAccounts.List"));
        subAccountsPermission.AddChild(
            EdaryPermissions.SubAccounts.Create,
            L("Permission:SubAccounts.Create"));
        subAccountsPermission.AddChild(
            EdaryPermissions.SubAccounts.Update,
            L("Permission:SubAccounts.Update"));
        subAccountsPermission.AddChild(
            EdaryPermissions.SubAccounts.Delete,
            L("Permission:SubAccounts.Delete"));

        // JournalEntries
        var journalEntriesPermission = edaryGroup.AddPermission(
            EdaryPermissions.JournalEntries.Default,
            L("Permission:JournalEntries"));
        journalEntriesPermission.AddChild(
            EdaryPermissions.JournalEntries.List,
            L("Permission:JournalEntries.List"));
        journalEntriesPermission.AddChild(
            EdaryPermissions.JournalEntries.Create,
            L("Permission:JournalEntries.Create"));
        journalEntriesPermission.AddChild(
            EdaryPermissions.JournalEntries.Update,
            L("Permission:JournalEntries.Update"));
        journalEntriesPermission.AddChild(
            EdaryPermissions.JournalEntries.Delete,
            L("Permission:JournalEntries.Delete"));

        // Invoices
        var invoicesPermission = edaryGroup.AddPermission(
            EdaryPermissions.Invoices.Default,
            L("Permission:Invoices"));
        invoicesPermission.AddChild(
            EdaryPermissions.Invoices.List,
            L("Permission:Invoices.List"));
        invoicesPermission.AddChild(
            EdaryPermissions.Invoices.Create,
            L("Permission:Invoices.Create"));
        invoicesPermission.AddChild(
            EdaryPermissions.Invoices.Update,
            L("Permission:Invoices.Update"));
        invoicesPermission.AddChild(
            EdaryPermissions.Invoices.Delete,
            L("Permission:Invoices.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EdaryResource>(name);
    }
}
