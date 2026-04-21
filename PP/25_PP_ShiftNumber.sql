-- ============================================================
-- PP Preprocess: Shift 1 / Shift 2 toggle
-- Adds ShiftNumber column to PP_PreprocessShift and backfills
-- historical rows: first shift each (Product, Date) = 1, toggle thereafter.
-- ============================================================

ALTER TABLE PP_PreprocessShift
    ADD COLUMN ShiftNumber TINYINT UNSIGNED NULL AFTER Status;

-- Backfill: for each (ProductID, ShiftDate), number shifts in start order,
-- then ShiftNumber = 1 if odd rank, 2 if even.
UPDATE PP_PreprocessShift s
JOIN (
    SELECT ShiftID,
           ((ROW_NUMBER() OVER (PARTITION BY ProductID, ShiftDate
                                ORDER BY StartTime, ShiftID) - 1) % 2) + 1 AS Num
      FROM PP_PreprocessShift
) r ON r.ShiftID = s.ShiftID
   SET s.ShiftNumber = r.Num;
