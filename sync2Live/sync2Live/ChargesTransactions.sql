DECLARE @i bigINT
DECLARE @jj bigINT
DECLARE @j table([id] bigINT)
DECLARE @PreTbl TABLE ([Id] BIGINT, [liveID] BIGINT)

insert into @PreTbl select id, NULL
from charges  where liveID is null 

/******************/

set @i = isnull((SELECT top 1 id FROM charges where liveId is null order by Id ),0)

while @i <= isnull((SELECT MAX(id) as max_ID FROM charges where liveID is null),0)
begin

	INSERT INTO [CollectionSystem_Test].dbo.Charges
							 ( DateOfService, DateOfPosting, PracticeId, PatientId, CPTId, InsuranceCompanyId, Billed, Payment, Adjustment, Balance, UpdateDate, CreateDate, DoctorId, CaseNumber, StatusId, DueDate, ImportID)
	output inserted.id into @j 
	SELECT         DateOfService, DateOfPosting, PracticeId, PatientId, CPTId, InsuranceCompanyId, Billed, Payment, Adjustment, Balance, UpdateDate, CreateDate, DoctorId, CaseNumber, StatusId, DueDate, ID
	FROM            [CollectionSystem-Imports].dbo.Charges 
	where  id = @i
	set @jj = (select top 1 id from @j)
	update charges set LiveID = @jj, updated =0 where id = @i
	update @preTbl set liveID = @jj where id = @i
	set @i = @i+1
	delete from @j
end

delete from [CollectionSystem_Test].dbo.Transactions where chargeID in (select liveID from @preTbl)

INSERT INTO [CollectionSystem_Test].dbo.Transactions
 ( DateOfService, DateOfPosting, TransactionId, Billed, Payment, Adjustment, Balance, UpdateDate, CreateDate, ChargeId)
SELECT        Transactions_1.DateOfService, Transactions_1.DateOfPosting, Transactions_1.TransactionId, Transactions_1.Billed, Transactions_1.Payment, 
                         Transactions_1.Adjustment, Transactions_1.Balance, Transactions_1.UpdateDate, Transactions_1.CreateDate, pTbl.LiveId
FROM            Transactions AS Transactions_1 INNER JOIN
                         @PreTbl pTbl ON Transactions_1.ChargeId = pTbl.Id





delete from @PreTbl
insert into @PreTbl select id, LiveID
from charges  where updated = 1 and liveID > 0

delete from [CollectionSystem_Test].dbo.Transactions where chargeID in (select liveID from @preTbl)

set @i = isnull((SELECT top 1 id FROM charges where updated=1 order by Id ),0)

while @i <= isnull((SELECT MAX(id) as max_ID FROM charges where updated=1),0)
begin

	if isnull ((select top 1 id from @preTBL where id = @i),0) >0 
	begin 
		set @jj = (select LiveID from @preTBL where id = @i)
		UPDATE       [CollectionSystem_Test].dbo.Charges
		SET   Payment = isnull((select payment from charges where id=@i),0), Adjustment =isnull((select adjustment from charges where id=@i),0), Balance = isnull((select balance from charges where id=@i),0), updateDate = isnull((select updateDate from charges where id=@i),0), importID = @i
			where id = @jj
	end
		set @i = @i+1
end


INSERT INTO [CollectionSystem_Test].dbo.Transactions
 ( DateOfService, DateOfPosting, TransactionId, Billed, Payment, Adjustment, Balance, UpdateDate, CreateDate, ChargeId)
SELECT        Transactions_1.DateOfService, Transactions_1.DateOfPosting, Transactions_1.TransactionId, Transactions_1.Billed, Transactions_1.Payment, 
                         Transactions_1.Adjustment, Transactions_1.Balance, Transactions_1.UpdateDate, Transactions_1.CreateDate, pTbl.LiveId
FROM            Transactions AS Transactions_1 INNER JOIN
                         @PreTbl pTbl ON Transactions_1.ChargeId = pTbl.Id


update charges set updated =0 