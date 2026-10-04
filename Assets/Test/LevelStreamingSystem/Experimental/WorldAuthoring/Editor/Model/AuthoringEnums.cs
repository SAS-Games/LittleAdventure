namespace SAS.WorldStreaming.Editor
{
    public enum StreamingRegionGenerationMode
    {
        UniformGrid,
        ManualRegions
    }

    public enum BoundsFallbackMode
    {
        TransformPosition,
        FixedSizeBounds,
        RequireManualAssignment,
        Ignore
    }

    public enum RegionAssignmentStrategy
    {
        PivotPosition,
        BoundsCenter,
        MaximumBoundsOverlap,
        FullyContainedOnly
    }

    public enum CrossRegionObjectPolicy
    {
        ReportOnly,
        AssignByMaximumOverlap,
        MoveToPersistentScene,
        AssignToSharedScene,
        RequireManualAssignment
    }

    public enum HierarchyAssignmentPolicy
    {
        RootOnly,
        NearestAuthoringRoot,
        PrefabInstanceRoot,
        ExplicitGroupRoot,
        IndividualObjects
    }

    public enum NeighborMode
    {
        Cardinal4,
        CardinalAndDiagonal8,
        BoundsIntersection,
        Custom
    }

    public enum WorldObjectClassification
    {
        Unassigned,
        Persistent,
        StreamingRegion,
        SharedScene,
        Ignored
    }

    public enum StreamingAssignmentMode
    {
        Automatic,
        Persistent,
        SpecificRegion,
        SharedScene,
        Ignore
    }

    public enum WorldObjectAssignmentSource
    {
        Automatic,
        PersistentRule,
        ManualOverride,
        CrossRegionPolicy
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
}
