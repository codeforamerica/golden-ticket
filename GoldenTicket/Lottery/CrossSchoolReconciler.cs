using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoldenTicket.DAL;
using GoldenTicket.Misc;
using GoldenTicket.Models;

namespace GoldenTicket.Lottery
{
    public class CrossSchoolReconciler
    {
        private GoldenTicketDbContext db;
        private SchoolLottery lottery;

        public CrossSchoolReconciler(GoldenTicketDbContext db)
        {
            this.db = db;
            lottery = new SchoolLottery(db);
        }

        public void Reconcile()
        {
            while (true)
            {
                var selecteds = db.Selecteds.ToList();

                // Selected applicants can't be waitlisted anywhere
                var waitlisteds = db.Waitlisteds.ToList();
                var waitlistedsToRemove = new List<Waitlisted>();
                selecteds.ForEach(x => waitlistedsToRemove.AddRange(waitlisteds.Where(y => y.ApplicantID == x.ApplicantID)));
                db.Waitlisteds.RemoveRange(waitlistedsToRemove);
                db.SaveChanges();

                // Find out if there are any applicants that need to be reconciled
                var selectedCount = new Dictionary<int, int>(); // key => selected applicant ID, # of schools selected for
                foreach (var selected in selecteds)
                {
                    var count = 0;
                    selectedCount.TryGetValue(selected.ApplicantID, out count);
                    selectedCount[selected.ApplicantID] = ++count;
                }

                // --- Exit case: all selected students are only selected for one school ---
                var multiSelectedApplicantCounts = selectedCount.Where(x => x.Value > 2).ToList();
                if (!multiSelectedApplicantCounts.Any())
                {
                    return;
                }

                // Applicants can only be selected once
                // Remove selected applicants from schools with a worse rank
                var schoolIdsToRerun = new List<int>();
                foreach (var pair in multiSelectedApplicantCounts)
                {
                    var applicantId = pair.Key;
                    var worseRankSchoolsForApplicant = RemoveWorseRankSelecteds(applicantId);
                    schoolIdsToRerun.AddRange(worseRankSchoolsForApplicant);
                }
                db.SaveChanges();

                // Re-run lottery for schools that had a selected removed
                var uniqueSchoolIdsToRerun = new HashSet<int>(schoolIdsToRerun); // eliminate school IDs appearing twice
                foreach (var schoolId in uniqueSchoolIdsToRerun)
                {
                    var school = db.Schools.Find(schoolId);
                    lottery.Run(school, Utils.GetApplicants(school.Waitlisteds), false);
                }
            }
        }

        private IEnumerable<int> RemoveWorseRankSelecteds(int applicantId)
        {
            // The best rank selected will be the first in order, the others should be removed
            var selecteds = db.Selecteds.Where(x => x.ApplicantID == applicantId).OrderBy(x => x.Rank).ToList();
            var worseRankSelecteds = selecteds.GetRange(1, selecteds.Count - 1);
            var schoolsRemovedFrom = new List<int>();
            
            // Identify the schools for the worse rank selecteds for the applicant
            worseRankSelecteds.ForEach(x => schoolsRemovedFrom.Add(x.SchoolID));

            // Remove the selecteds
            db.Selecteds.RemoveRange(worseRankSelecteds);

            return schoolsRemovedFrom;
        }

    }
}