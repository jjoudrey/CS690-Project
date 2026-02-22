namespace MiaLearningSystem;

public class DataManager
{
    private const string CoursesFile = "courses.txt";
    private const string TopicsFile = "topics.txt";
    private const string NotesIndexFile = "notes.txt";
    private const string QuizzesFile = "quizzes.txt";
    private const string QuizTopicsFile = "quiz_topics.txt";
    public const string NotesDirectory = "Notes";

    public List<Course> Courses { get; } = [];
    public List<Topic> Topics { get; } = [];
    public List<Note> Notes { get; } = [];
    public List<Quiz> Quizzes { get; } = [];
    public List<QuizTopic> QuizTopics { get; } = [];

    public DataManager()
    {
        Directory.CreateDirectory(NotesDirectory);
        LoadCourses();
        LoadTopics();
        LoadNotes();
        LoadQuizzes();
        LoadQuizTopics();
    }

    private void LoadCourses()
    {
        if (!File.Exists(CoursesFile)) return;

        foreach (var line in File.ReadAllLines(CoursesFile))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(':', 3);
            if (parts.Length < 3) continue;
            Courses.Add(new Course(Guid.Parse(parts[0]), parts[1], parts[2]));
        }
    }

    public void AddCourse(Course course)
    {
        Courses.Add(course);
        SynchronizeCourses();
    }

    public void UpdateCourse(Guid courseId, string newName, string newSubjectArea)
    {
        int index = Courses.FindIndex(c => c.CourseId == courseId);
        if (index < 0) return;
        Courses[index] = new Course(courseId, newName, newSubjectArea);
        SynchronizeCourses();
    }

    public void RemoveCourse(Course course)
    {
        Courses.Remove(course);
        SynchronizeCourses();
    }

    private void SynchronizeCourses()
    {
        File.Delete(CoursesFile);
        foreach (var course in Courses)
            File.AppendAllText(CoursesFile, course.ToString() + Environment.NewLine);
    }

    private void LoadTopics()
    {
        if (!File.Exists(TopicsFile)) return;

        foreach (var line in File.ReadAllLines(TopicsFile))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(':', 3);
            if (parts.Length < 3) continue;
            Topics.Add(new Topic(Guid.Parse(parts[0]), Guid.Parse(parts[1]), parts[2]));
        }
    }

    public void AddTopic(Topic topic)
    {
        Topics.Add(topic);
        SynchronizeTopics();
    }

    public void UpdateTopic(Guid topicId, string newName)
    {
        int index = Topics.FindIndex(t => t.TopicId == topicId);
        if (index < 0) return;
        Topics[index] = new Topic(topicId, Topics[index].CourseId, newName);
        SynchronizeTopics();
    }

    public void RemoveTopic(Topic topic)
    {
        Topics.Remove(topic);
        SynchronizeTopics();
    }

    private void SynchronizeTopics()
    {
        File.Delete(TopicsFile);
        foreach (var topic in Topics)
            File.AppendAllText(TopicsFile, topic.ToString() + Environment.NewLine);
    }

    private void LoadNotes()
    {
        if (!File.Exists(NotesIndexFile)) return;

        foreach (var line in File.ReadAllLines(NotesIndexFile))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(':', 4);
            if (parts.Length < 4) continue;
            Notes.Add(new Note(Guid.Parse(parts[0]), Guid.Parse(parts[1]), parts[2], parts[3]));
        }
    }

    public void AddNote(Note note)
    {
        Notes.Add(note);
        SynchronizeNotes();
    }

    public void UpdateNote(Guid noteId, string newName)
    {
        int index = Notes.FindIndex(n => n.NoteId == noteId);
        if (index < 0) return;
        var n = Notes[index];
        Notes[index] = new Note(noteId, n.TopicId, n.FileName, newName);
        SynchronizeNotes();
    }

    public void RemoveNote(Note note)
    {
        Notes.Remove(note);
        SynchronizeNotes();
        string filePath = NoteFilePath(note);
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    private void SynchronizeNotes()
    {
        File.Delete(NotesIndexFile);
        foreach (var note in Notes)
            File.AppendAllText(NotesIndexFile, note.ToString() + Environment.NewLine);
    }

    private void LoadQuizzes()
    {
        if (!File.Exists(QuizzesFile)) return;

        foreach (var line in File.ReadAllLines(QuizzesFile))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(':', 5);
            if (parts.Length < 5) continue;
            Quizzes.Add(new Quiz(
                Guid.Parse(parts[0]),
                Guid.Parse(parts[1]),
                parts[4],
                DateOnly.Parse(parts[2]),
                bool.Parse(parts[3])
            ));
        }
    }

    public void AddQuiz(Quiz quiz)
    {
        Quizzes.Add(quiz);
        SynchronizeQuizzes();
    }

    public void MarkQuizCompleted(Guid quizId)
    {
        int index = Quizzes.FindIndex(q => q.QuizId == quizId);
        if (index < 0) return;
        var q = Quizzes[index];
        Quizzes[index] = new Quiz(q.QuizId, q.CourseId, q.Name, q.DueDate, true);
        SynchronizeQuizzes();
    }

    public void UpdateQuiz(Guid quizId, string newName, DateOnly newDueDate)
    {
        int index = Quizzes.FindIndex(q => q.QuizId == quizId);
        if (index < 0) return;
        var q = Quizzes[index];
        Quizzes[index] = new Quiz(quizId, q.CourseId, newName, newDueDate, q.IsCompleted);
        SynchronizeQuizzes();
    }

    public void RemoveQuiz(Quiz quiz)
    {
        Quizzes.Remove(quiz);
        QuizTopics.RemoveAll(qt => qt.QuizId == quiz.QuizId);
        SynchronizeQuizzes();
        SynchronizeQuizTopics();
    }

    private void SynchronizeQuizzes()
    {
        File.Delete(QuizzesFile);
        foreach (var quiz in Quizzes)
            File.AppendAllText(QuizzesFile, quiz.ToString() + Environment.NewLine);
    }

    private void LoadQuizTopics()
    {
        if (!File.Exists(QuizTopicsFile)) return;

        foreach (var line in File.ReadAllLines(QuizTopicsFile))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(':', 3);
            if (parts.Length < 3) continue;
            QuizTopics.Add(new QuizTopic(Guid.Parse(parts[0]), Guid.Parse(parts[1]), Guid.Parse(parts[2])));
        }
    }

    public void AddQuizTopic(QuizTopic quizTopic)
    {
        QuizTopics.Add(quizTopic);
        SynchronizeQuizTopics();
    }

    public void ReplaceQuizTopics(Guid quizId, IEnumerable<Guid> topicIds)
    {
        QuizTopics.RemoveAll(qt => qt.QuizId == quizId);
        foreach (var topicId in topicIds)
            QuizTopics.Add(new QuizTopic(Guid.NewGuid(), quizId, topicId));
        SynchronizeQuizTopics();
    }

    private void SynchronizeQuizTopics()
    {
        File.Delete(QuizTopicsFile);
        foreach (var qt in QuizTopics)
            File.AppendAllText(QuizTopicsFile, qt.ToString() + Environment.NewLine);
    }

    public static string NoteFilePath(Note note) =>
        Path.Combine(NotesDirectory, note.FileName);

    public static void AppendNoteLine(Note note, string line) =>
        File.AppendAllText(NoteFilePath(note), line + Environment.NewLine);

    // Returns lines from the note file. Returns null if the file fails ASCII validation.
    public static string[]? ReadNoteLines(Note note, out string validationError)
    {
        validationError = string.Empty;
        string path = NoteFilePath(note);

        if (!File.Exists(path))
        {
            validationError = "Note file not found.";
            return null;
        }

        byte[] bytes = File.ReadAllBytes(path);
        foreach (byte b in bytes)
        {
            if (b > 127)
            {
                validationError = "Note file contains non-ASCII characters.";
                return null;
            }
        }

        return File.ReadAllLines(path);
    }
}
