namespace Inpad.Api.Models;

public enum ObjectStatus
{
    Draft,
    UnderReview,
    NeedsRevision,
    Published,
    Archived,
    PublishError
}

public enum WordPressStatus
{
    NotPublished,
    WordPressDraft,
    Published,
    Updated,
    Unpublished,
    PublishError
}

public enum UserRole
{
    Administrator,
    Editor,
    Manager,
    Viewer
}

public enum MediaTag
{
    Website,
    Presentation,
    Portfolio
}

public enum MediaType
{
    MainImage,
    Gallery,
    Render,
    Plan,
    Photo,
    PresentationCover
}

public enum ProjectStatus
{
    SketchProject, ProjectDocumentation, WorkingDocumentation,
    Approval, UnderConstruction, Realized, Reconstruction,
    Completed, Archived, Frozen, Cancelled
}

public enum DesignStage { Concept, SketchProject, P, R }
