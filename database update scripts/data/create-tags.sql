update datasetversions
set tagref = NULL;

delete from tags;
select id,datasetref, tagref, show from datasetversions

DO $$
DECLARE
    dataset RECORD;
	datasetversion RECORD;
	i int  := 0;
    tag numeric  := 0;
	decimalpart numeric := 0;
	tagAsTExt text:='';
	tagid int:=0;

	
BEGIN

	 FOR dataset IN SELECT id FROM datasets order by id LOOP
		i := 0;
		tag:= 0;
		decimalpart:=0;
       FOR datasetversion IN SELECT * FROM datasetversions where datasetref = dataset.id order by id LOOP
			i := i + 1;

			if i = (SELECT count(id) FROM datasetversions where datasetref = dataset.id) THEN
				tag:= 1; 
			ELSE
				decimalpart:=decimalpart+1;
				tagAsTExt := '0.'|| Cast(decimalpart as text);
				tag:=CAST(tagAsTExt as numeric);
			END IF;

		   INSERT INTO tags (nr,releasedate,final)  
			VALUES (tag,datasetversion.timestamp,true)
			RETURNING id into tagid;
		
		    update datasetversions
				set 
				tagref = tagid, 
				show = true
				where id = datasetversion.id;
			
	        -- Hier die Aktionen für jede Zeile ausführen
	        RAISE NOTICE 'dataset: %; version: %; nr: %;tag: %', dataset.id,datasetversion.id, i, tag ;
	        -- Weitere Operationen mit row_data
		END LOOP;
    END LOOP;

END $$;