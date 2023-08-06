using System;

public record StartConditionRecord(
    bool PreCondition, bool ArticlesProvided, DateTime WasSetReadyAt) {
    public bool Satisfied => PreCondition && ArticlesProvided;
}