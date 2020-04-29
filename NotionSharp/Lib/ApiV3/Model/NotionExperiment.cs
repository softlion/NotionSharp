namespace NotionSharp.Lib.ApiV3.Model
{
    public class NotionExperiment
    {
        /// <summary>
        /// personal-onboarding, save-transactions-legacy, alpha-api, better-notification-triage, multi-account, content-classification-block, always-send-email, use-case-onboarding, better-mobile, remote-landing, better-search,
        /// case-studies, save-transactions-memory, save-transactions-indexdb, auth-redirect, password, startup-landing, student-marketing, student, saml
        /// </summary>
        public string ExperimentId { get; set; }
        public int ExperimentVersion { get; set; }
        /// <summary>
        /// (null), control, preview, has_startup_landing, browser, has_case_studies, has_remote_landing, has_better_mobile, has_new_onboarding_no_tooltips, has-content-classification-block, on
        /// </summary>
        public string Group { get; set; }
    }
}
