# Step 1: Schema for all tables
mysqldump -u root -p -P 3308 --no-data --routines --triggers --events `
  --skip-comments --set-gtid-purged=OFF `
  stockdb > "C:\Code\Schema Dump\stockdb_dump.sql"

# Step 2: Append data for masters + semi-masters + sequences
mysqldump -u root -p -P 3308 --no-create-info --skip-comments --set-gtid-purged=OFF `
  stockdb `
    users cities states `
    erp_applications erp_modules erp_roles erp_roleappaccess erp_rolemoduleaccess `
    mm_suppliers mm_rawmaterials mm_packingmaterials mm_consumables mm_stationaries `
    mm_scrapmaterials mm_pmcategories mm_uom mm_useraccess `
    mm_openingstock mm_rmscraplink mm_scrappricehistory `
    mm_grncounter mm_grnsequence `
    pk_customers pk_customertypes pk_machines pk_productmrp pk_productpmmaster `
    pk_customermargins pk_customerproductmargins pk_fgopeningstock `
    pk_invoicesequence `
    pp_products pp_productionlines pp_bom pp_productparams pp_batchparams `
    pp_preprocessstages pp_fgpackingoptions pp_remarkoptions `
    fin_chartofaccounts fin_services fin_serviceprovider_services `
    fin_bankaccounts fin_banklayouts `
    fin_partyopeningbalance `
    fin_tallycustomermap fin_tallyitemmap fin_tallyproductmap fin_tallyscrapmap fin_tallysuppliermap `
    fin_journalsequence `
    sa_areas sa_channels sa_designations sa_orgpositions sa_productdefaultpack `
    sa_regions sa_transportmodes sa_zones `
    zoho_config `
    zoho_consitemmap zoho_customermap zoho_itemmap zoho_pmitemmap `
    zoho_rmitemmap zoho_statitemmap zoho_suppliermap `
  >> "C:\Code\Schema Dump\stockdb_dump.sql"