namespace MiaLearningSystem;

public class Course
{
    public Guid CourseId { get; }
    public string Name { get; }
    public string SubjectArea { get; }

    public Course(Guid courseId, string name, string subjectArea)
    {
        CourseId = courseId;
        Name = name;
        SubjectArea = subjectArea;
    }

    public override string ToString()
    {
        return $"{CourseId}:{Name}:{SubjectArea}";
    }
}

public class Topic
{
    public Guid TopicId { get; }
    public Guid CourseId { get; }
    public string Name { get; }

    public Topic(Guid topicId, Guid courseId, string name)
    {
        TopicId = topicId;
        CourseId = courseId;
        Name = name;
    }

    public override string ToString()
    {
        return $"{TopicId}:{CourseId}:{Name}";
    }
}

public class Note
{
    public Guid NoteId { get; }
    public Guid TopicId { get; }
    public string FileName { get; }
    public string Name { get; }

    public Note(Guid noteId, Guid topicId, string fileName, string name)
    {
        NoteId = noteId;
        TopicId = topicId;
        FileName = fileName;
        Name = name;
    }

    // Format: noteId:topicId:fileName:name  (name is last so it can contain colons)
    public override string ToString()
    {
        return $"{NoteId}:{TopicId}:{FileName}:{Name}";
    }
}

public class Quiz
{
    public Guid QuizId { get; }
    public Guid CourseId { get; }
    public string Name { get; }
    public DateOnly DueDate { get; }
    public bool IsCompleted { get; }

    public Quiz(Guid quizId, Guid courseId, string name, DateOnly dueDate, bool isCompleted)
    {
        QuizId = quizId;
        CourseId = courseId;
        Name = name;
        DueDate = dueDate;
        IsCompleted = isCompleted;
    }

    // Format: guid:courseId:dueDate:isCompleted:name  (name is last — can contain colons)
    public override string ToString()
    {
        return $"{QuizId}:{CourseId}:{DueDate:yyyy-MM-dd}:{IsCompleted}:{Name}";
    }
}

public class QuizTopic
{
    public Guid QuizTopicId { get; }
    public Guid QuizId { get; }
    public Guid TopicId { get; }

    public QuizTopic(Guid quizTopicId, Guid quizId, Guid topicId)
    {
        QuizTopicId = quizTopicId;
        QuizId = quizId;
        TopicId = topicId;
    }

    public override string ToString()
    {
        return $"{QuizTopicId}:{QuizId}:{TopicId}";
    }
}

public class ReferenceList
{
    public Guid ReferenceListId { get; }
    public Guid CourseId { get; }
    public string Name { get; }

    public ReferenceList(Guid referenceListId, Guid courseId, string name)
    {
        ReferenceListId = referenceListId;
        CourseId = courseId;
        Name = name;
    }

    // Format: referenceListId:courseId:name  (name is last so it can contain colons)
    public override string ToString()
    {
        return $"{ReferenceListId}:{CourseId}:{Name}";
    }
}

public class ReferenceEntry
{
    public Guid EntryId { get; }
    public Guid ReferenceListId { get; }
    public string Term { get; }
    public string Definition { get; }

    public ReferenceEntry(Guid entryId, Guid referenceListId, string term, string definition)
    {
        EntryId = entryId;
        ReferenceListId = referenceListId;
        Term = term;
        Definition = definition;
    }

    // Format: entryId:referenceListId:term:definition  (definition is last so it can contain colons)
    public override string ToString()
    {
        return $"{EntryId}:{ReferenceListId}:{Term}:{Definition}";
    }
}
