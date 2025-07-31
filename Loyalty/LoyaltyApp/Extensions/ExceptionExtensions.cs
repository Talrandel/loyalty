namespace LoyaltyApp.Extensions;

public static class ExceptionExtensions
{
    public static string GetFullExceptionInfo(this Exception ex)
    {
        var sb = new System.Text.StringBuilder();

        AppendException(ex, 0);
        return sb.ToString();

        void AppendException(Exception e, int level)
        {
            while (true)
            {
                var indent = new string('>', level * 2);
                sb.AppendLine($"{indent}Exception: {e.GetType().FullName}");
                sb.AppendLine($"{indent}Message: {e.Message}");
                sb.AppendLine($"{indent}StackTrace:");
                sb.AppendLine($"{indent}{e.StackTrace}");

                if (e is AggregateException aggEx)
                {
                    var i = 1;
                    foreach (var inner in aggEx.InnerExceptions)
                    {
                        sb.AppendLine($"{indent}-- Aggregate Inner #{i} --");
                        AppendException(inner, level + 1);
                        i++;
                    }
                }
                else if (e.InnerException != null)
                {
                    sb.AppendLine($"{indent}-- Inner Exception --");
                    e = e.InnerException;
                    level += 1;
                    continue;
                }

                break;
            }
        }
    }
}