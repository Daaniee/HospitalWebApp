namespace hospitalwebapp.Models
{
    public enum VisitStatus
    {
        Pending,      // Patient is waiting
        InProgress,   // Doctor is currently seeing the patient
        Completed     // Consultation is finished
    }
}
