namespace CriminalCase2.Data
{
    public static class SuspectRoleExtensions
    {
        public static string ToDisplayName(this SuspectRole role)
        {
            return role switch
            {
                SuspectRole.User => "Pecandu",
                SuspectRole.Dealer => "Bandar Narkoba",
                SuspectRole.Normal => "Warga Biasa",
                _ => role.ToString()
            };
        }
    }

    public static class DrugTestResultExtensions
    {
        public static string ToDisplayName(this DrugTestResult result)
        {
            return result switch
            {
                DrugTestResult.Positive => "Positif",
                DrugTestResult.Negative => "Negatif",
                _ => result.ToString()
            };
        }
    }
}