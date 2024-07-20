(function ($) {
 "use strict";
	
	$(document).ready(function() {
		$('#data-table-basic').DataTable();

		$('#StationeryItem').DataTable({
			"order": [[7, 'desc']],
		});


	
		$('#EmployeeTable').DataTable({
			"order": [[0, 'desc']],
		});

		$('#SubmitedRequest').DataTable({
			"order": [[1, 'desc']],
		});

		$('#ReceivedRequest').DataTable({
			"order": [[1, 'desc']],
		});



		$('#ApprovedRequest').DataTable({
			"order": [[1, 'desc']],
		});


		$('#ListRestock').DataTable();
		$('#ListStock').DataTable({
			"order": [[0, 'desc']],
		});
		$('#ListUseless').DataTable({
			"order": [[0, 'desc']],
		});
		$('#ListRequestTable').DataTable({
			"order": [[3, 'desc']],
		});
	
		$('#ListStockLevelWarning').DataTable({
			"order": [[5, 'asc']],
		});
		$('#ListUselessWarning').DataTable({
			"order": [[4, 'asc']],
		});


		$('#Brand').DataTable();
		$('#Category').DataTable();
		$('#detailbrand').DataTable();
		$('#DetailCate').DataTable();
		$('#DetailSta').DataTable();
		$('#hidden').DataTable();
		
	});
	
})(jQuery); 

