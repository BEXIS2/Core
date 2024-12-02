DO
$do$
DECLARE

dwclinks text[] := ARRAY[
'siteCount',
 'siteNestingDescription',
 'verbatimSiteDescriptions',
 'verbatimSiteNames',
 'geospatialScopeAreaValue',
 'geospatialScopeAreaUnit',
 'totalAreaSampledValue',
 'totalAreaSampledUnit',
 'reportedWeather',
 'reportedExtremeConditions',
 'targetHabitatScope',
 'excludedHabitatScope',
 'eventDurationValue',
 'eventDurationUnit',
 'targetTaxonomicScope',
 'excludedTaxonomicScope',
 'taxonCompletenessReported',
 'taxonCompletenessProtocols',
 'isTaxonomicScopeFullyReported',
 'isAbsenceReported',
 'absentTaxa',
 'hasNonTargetTaxa',
 'nonTargetTaxa',
 'areNonTargetTaxaFullyReported',
 'targetLifeStageScope',
 'excludedLifeStageScope',
 'isLifeStageScopeFullyReported',
 'targetDegreeOfEstablishmentScope',
 'excludedDegreeOfEstablishmentScope',
 'isDegreeOfEstablishmentScopeFullyReported',
 'targetGrowthFormScope',
 'excludedGrowthFormScope',
 'isGrowthFormScopeFullyReported',
 'hasNonTargetOrganisms',
 'verbatimTargetScope',
 'compilationTypes',
 'compilationSourceTypes',
 'inventoryTypes',
 'protocolNames',
 'protocolDescriptions',
 'protocolReferences',
 'isAbundanceReported',
 'isAbundanceCapReported',
 'abundanceCap',
 'isVegetationCoverReported',
 'isLeastSpecificTargetCategoryQuantityInclusive',
 'hasVouchers',
 'voucherInstitutions',
 'hasMaterialSamples',
 'materialSampleTypes',
 'samplingPerformedBy',
 'isSamplingEffortReported',
 'samplingEffortProtocol',
 'samplingEffortValue',
 'samplingEffortUnit',
 'absentTaxa',
 'compilationSourceTypes',
 'compilationTypes',
 'eventDurationUnit',
 'excludedDegreeOfEstablishmentScope',
 'excludedGrowthFormScope',
 'excludedHabitatScope',
 'excludedLifeStageScope',
 'excludedTaxonomicScope',
 'geospatialScopeAreaUnit',
 'inventoryTypes',
 'materialSampleTypes',
 'nonTargetTaxa',
 'protocolNames',
 'samplingEffortProtocol',
 'samplingEffortUnit',
 'samplingPerformedBy',
 'targetDegreeOfEstablishmentScope',
 'targetGrowthFormScope',
 'targetHabitatScope',
 'targetLifeStageScope',
 'targetTaxonomicScope',
 'taxonCompletenessProtocols',
 'taxonCompletenessReported',
 'totalAreaSampledUnit',
 'absentTaxa',
 'compilationSourceTypes',
 'compilationTypes',
 'eventDurationUnit',
 'excludedDegreeOfEstablishmentScope',
 'excludedGrowthFormScope',
 'excludedHabitatScope',
 'excludedLifeStageScope',
 'excludedTaxonomicScope',
 'geospatialScopeAreaUnit',
 'inventoryTypes',
 'materialSampleTypes',
 'nonTargetTaxa',
 'protocolNames',
 'samplingEffortProtocol',
 'samplingEffortUnit',
 'samplingPerformedBy',
 'targetDegreeOfEstablishmentScope',
 'targetGrowthFormScope',
 'targetHabitatScope',
 'targetLifeStageScope',
 'targetTaxonomicScope',
 'taxonCompletenessProtocols',
 'taxonCompletenessReported',
 'totalAreaSampledUnit'       
];

begin

-- dwc
INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT 'dwc_eco','http://rs.tdwg.org/eco/terms/',1, null, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name='dwc_eco');

INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT 'hasDwcTerm','na', 6, null, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name='hasDwcTerm');

for i in array_lower(dwclinks, 1)..array_upper(dwclinks, 1) loop

RAISE NOTICE 'name: %',dwclinks[i];

INSERT INTO public.rpm_externallink(name, uri, type, prefix, prefixcategory, versionno)
SELECT dwclinks[i],dwclinks[i], 5, 1, null,1
WHERE NOT EXISTS (SELECT * FROM public.rpm_externallink WHERE name=dwclinks[i]);

end loop;

END
$do$;
