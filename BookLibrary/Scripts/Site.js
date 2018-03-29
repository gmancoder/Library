String.prototype.toTitleCase = function (n) {
    var s = this;
    if (1 !== n) s = s.toLowerCase();
    return s.replace(/(^|\s)[a-z]/g, function (f) { return f.toUpperCase() });
}

var tables = [];
var table_ids = [];
var loaders = [];
$(document).ready(function () {
    $('input.sw-checkbox').bootstrapSwitch({
        size: 'small',
        onText: 'Yes',
        offText: 'No',
    });

    $('#Lyrics').summernote();
    ResizeWindows();

    $('#user_table').DataTable({
        order: [[2, 'asc']],
        columns: [
            null,
            null,
            null,
            { 'searching': false, 'ordering': false }
        ]
    });
    $('#role_table').DataTable({
        order: [[1, 'asc']],
        columns: [
            null,
            null,
            { 'searching': false, 'ordering': false }
        ]
    });
    $('.colorbox_img').colorbox(
        {
            rel: 'colorbox',
            maxWidth: '100%',
            maxHeight: '100%'
        });
    $('.datepicker').datepicker({
        format: 'yyyy-mm-dd'
    })
});

$(window).resize(ResizeWindows);

function ResizeWindows() {
    var height = window.innerHeight - 375;

    $('#body_container').css('height', height + 'px');
}

function ShowLoader(action, id) {
    if (loaders.length == 0) {
        var title = document.title;
        title = '[Busy] ' + title;
        document.title = title;
    }
    $('#' + id).show();
    $('#' + id + '_action').html(action);
    loaders.push(id);
}
function HideLoader(id) {
    idx = loaders.indexOf(id);
    if (idx > -1) {
        loaders.splice(idx, 1);
    }
    if (loaders.length == 0) {
        var title = document.title;
        title = title.replace('[Busy] ', '');
        document.title = title;
    }
    $('#' + id).hide();
}

function ShowModalLoader(modal, action) {
    ShowLoader(action, modal + '_modal_loading');
    $('#' + modal + '_modal_content').hide();
}

function HideModalLoader(modal) {
    HideLoader(modal + '_modal_loading');
    $('#' + modal + '_modal_content').show()
}


function HandleErrors(obj) {
    errors = "";
    for (var e = 0; e < obj.Errors.length; e++) {
        errors += obj.Errors[e].Message + "\n";
    }
    return errors;
}

function CreateDynamicOption(value, text, selected) {
    var sel = ""
    if (typeof selected !== undefined && selected != undefined) {
        sel = "selected"
    }
    return "<option class='dynamic_option' value='" + value + "' " + sel + ">" + text + "</option>";
}
function CreateDynamicListOption(value, text) {
    return "<li class='dynamic_option' id='" + value + "'>" + text + "</li>";
}

function SingleDimArrayAsString(arr, delim) {
    var str = "";
    for (var f = 0; f < arr.length; f++) {
        if (str != "") {
            str += "|"
        }
        str += arr[f];
    }
    return str;
}

function PopulateDataTable(field_names, data, table_id) {
    table_id = typeof table_id !== 'undefined' ? table_id : 'de_data_table';
    var columns = [];

    for (var f = 0; f < field_names.length; f++) {
        columns.push({
            data: field_names[f],
            title: field_names[f]
        });
    }

    _DestroyDataTable(table_id);

    var de_table = $('#' + table_id).DataTable({
        "destroy": true,
        "data": data,
        "columns": columns
    });

    _AppendDataTable(de_table, table_id);
}

function _DestroyDataTable(table_id) {
    idx = arrayIndex(table_id, table_ids)
    if (idx != null) {
        tables[idx].destroy();
        tables.splice(idx, 1);
        table_ids.splice(idx, 1);
        $('#' + table_id + '_body').empty();
    }
}

function _DestroyOnlyDataTable(table_id) {
    idx = arrayIndex(table_id, table_ids)
    if (idx != null) {
        tables[idx].destroy();
        tables.splice(idx, 1);
        table_ids.splice(idx, 1);
    }
}

function _AppendDataTable(table, table_id) {
    tables.push(table);
    table_ids.push(table_id);
}

function arrayCompare(a1, a2) {
    if (a1.length != a2.length) return false;
    var length = a2.length;
    for (var i = 0; i < length; i++) {
        if (a1[i] !== a2[i]) return false;
    }
    return true;
}

function inArray(needle, haystack) {
    var length = haystack.length;
    for (var i = 0; i < length; i++) {
        if (typeof haystack[i] == 'object') {
            if (arrayCompare(haystack[i], needle)) return true;
        } else {
            if (haystack[i] == needle) return true;
        }
    }
    return false;
}

function arrayIndex(needle, haystack) {
    var length = haystack.length;
    for (var i = 0; i < length; i++) {
        if (haystack[i] == needle) {
            return i;
        }
    }
    return null;
}
function addMonths(date, months) {
    date.setMonth(date.getMonth() + months);
    return date;
}

function AjaxCall(ajax_url, data, method, success_callback, error_callback) {
    $.ajax({
        type: method,
        url: ajax_url,
        data: data,
        success: success_callback,
        error: error_callback
    });
}

function GenerateAlias(obj_id, alias_id) {
    var alias = $('#' + obj_id).val().toLowerCase().replace(/ /g, '_');
    alias_field = $('#' + alias_id);
    if (alias_field.length > 0) {
        current_alias = alias_field.val();
        if (current_alias == "") {
            $('#' + alias_id).val(alias);
        }
    }
}

function ResetModal(modal) {
    $('#' + modal).find('input').each(function () {
        $(this).empty();
    });
    $('#' + modal).find('select').each(function () {
        $(this).val('');
    });
}


function SubmitForm(form_type) {
    $('#' + form_type + "_form").submit();
}

function AjaxError(msg) {
    console.log(msg);
    alert(msg);
}

function ShowPDFSelection(letter) {
    $('#pdfModal').modal('show');
    GetPDFs(letter);
}
function GetPDFs(letter) {
    ShowModalLoader('pdf');
    var url = '/ajax/pdfs/get';
    var data = { 'letter': letter };

    AjaxCall(url, data, "POST", GetPDFsSuccess, AjaxError);
}

function GetPDFsSuccess(data) {
    $('#pdfs').empty();
    obj = JSON.parse(data);
    if (obj.StatusType == "OK") {
        var pdfs = obj.Results.PDFs;
        var html = '<div class="row">';
        for (var idx = 0; idx < pdfs.length; idx++) {
            if (idx > 0 && idx % 3 == 0) {
                html += '</div><div class="row">';
            }
            var pdf = pdfs[idx];
            html += '<div class="col-md-4"><a href="javascript:;" onClick="SelectPDF(\'' + pdf.Id + '\', \'' + pdf.Name + '\');">' + pdf.Name + '</a></div>';
            
        }
        html += '</div>';
        $('#pdfs').append(html);
        HideModalLoader('pdf');
    }
    else {
        alert(HandleErrors(obj));
    }
}

function GetPDF(Id)
{
    var url = '/ajax/pdfs/get';
    var data = { 'Id': Id };

    AjaxCall(url, data, "POST", GetPDFSuccess, AjaxError);
}

function GetPDFSuccess(data) {
    obj = JSON.parse(data);
    if (obj.StatusType == "OK") {
        var pdfs = obj.Results.PDFs;
        if (pdfs.length > 0)
        {
            var pdf = pdfs[0];
            SelectPDF(pdf.Id, pdf.Name);
        }
    }
    else {
        alert(HandleErrors(obj));
    }
}

function SelectPDF(id, name) {
    $('#PdfId').val(id);
    $('#pdf_name').val(name);
    $('#pdfModal').modal('hide');
}

function EntryTypeChanged() {
    $('.entry_fields').hide();
    var entry_type = $('#EntryType').val();
    if (entry_type == "Amazon") {
        $('#amazon_entry_fields').show();
    }
    else {
        $('#manual_entry_fields').show();
    }
}
function IsGroupChanged(show_modal) {
    var is_group = $('#IsGroup').val();
    if (is_group == "false") {
        $('#is_group_field').removeClass('col-lg-10').addClass('col-lg-9');
        $('#celebrity_refresh_button').show();
        if (show_modal) {
            ShowCelebritySelection('A');
        }
    }
    else {
        $('#celebrity_refresh_button').hide();
        $('#is_group_field').removeClass('col-lg-9').addClass('col-lg-10');
    }
}
function ShowCelebritySelection(letter) {
    $('#celebrityModal').modal('show');
    GetCelebrities(letter);
}
function GetCelebrities(letter) {
    ShowModalLoader('celebrity');
    var url = '/ajax/celebrities/get';
    var data = { 'letter': letter };

    AjaxCall(url, data, "POST", GetCelebritiesSuccess, AjaxError);
}

function GetCelebritiesSuccess(data) {
    $('#celebrities').empty();
    obj = JSON.parse(data);
    if (obj.StatusType == "OK") {
        var celebrities = obj.Results.Celebrities;
        var html = '<div class="row">';
        for (var idx = 0; idx < celebrities.length; idx++) {
            if (idx > 0 && idx % 3 == 0) {
                html += '</div><div class="row">';
            }
            var celebrity = celebrities[idx];
            html += '<div class="col-md-4"><a href="javascript:;" onClick="SelectCelebrity(\'' + celebrity.Id + '\', \'' + celebrity.Name + '\');">' + celebrity.Name + '</a></div>';

        }
        html += '</div>';
        $('#celebrities').append(html);
        HideModalLoader('celebrity');
    }
    else {
        alert(HandleErrors(obj));
    }
}

function SelectCelebrity(id, name) {
    $('#CelebrityId').val(id);
    $('#Name').val(name);
    $('#celebrityModal').modal('hide');
}
function NewOnSave() {
    $('#new_on_save').val(1);
}

function PopulateCategoryIndex(category_id) {
    $('#sub_category_book_listing').hide();
    ShowLoader("Loading Categories and Items", "category_index_loading");

    var url = "/ajax/category/load";
    var data = { 'category_id': category_id };
    AjaxCall(url, data, "POST", PopulateCategoryIndexSuccess, AjaxError);
}
function PopulateCategoryIndexSuccess(data) {
    $('#sub_category_listing').empty();
    obj = JSON.parse(data);
    if (obj.StatusType == "OK") {
        var results = obj.Results.Items;
        var html = '<div class="row">';
        for (var idx = 0; idx < results.length; idx++) {
            var result = results[idx];
            if (idx % 4 == 0) {
                html += '</div><div class="row">';
            }
            html += '<div class="col-md-3" style="text-align:center;">';
            if (result.ResultType == "Category") {
                html += '<a href="/Categories/Details/' + result.Id + '">' +
                    '<img src="/Content/Images/folder.png" width="150" alt="' + result.Name + '" /><br />' +
                    result.Name +
                    '</a>';
            }
            else {
                html += '<a href="/' + result.ResultType +'/Details/' + result.Id + '">' +
                    '<img src="' + result.Image + '" width="150" alt="' + result.Name + '" /><br />' +
                    result.Name +
                    '</a>';
            }
            html += '</div>';
        }
        html += '</div>';
        $('#sub_category_listing').append(html);
        $('#sub_category_listing').show();
    }
    else {
        alert(HandleErrors(obj));
    }
    $('#sub_category_book_listing').show();
    HideLoader("category_index_loading");
}

function AddAssociatedArtist() {
    $('#associatedArtistModal').modal('show');
    GetAssociatedArtists('A');
}

function GetAssociatedArtists(letter) {
    ShowModalLoader('associatedArtist');
    var url = '/ajax/artist/get';
    var data = { 'letter': letter };

    AjaxCall(url, data, "POST", GetAssociatedArtistSuccess, AjaxError);
}

function GetAssociatedArtistSuccess(data) {
    $('#associatedArtists').empty();
    obj = JSON.parse(data);
    if (obj.StatusType == "OK") {
        var artists = obj.Results.Artists;
        var html = '<div class="row">';
        for (var idx = 0; idx < artists.length; idx++) {
            if (idx > 0 && idx % 3 == 0) {
                html += '</div><div class="row">';
            }
            var artist = artists[idx];
            if (artist.IsGroup) {
                html += '<div class="col-md-4"><a href="javascript:;" onClick="SelectArtist(\'' + artist.Id + '\', \'' + artist.Name + '\', \'' + artist.DisplayImage + '\');">' + artist.Name + '</a></div>';
            }
            else {
                html += '<div class="col-md-4"><a href="javascript:;" onClick="SelectArtist(\'' + artist.Id + '\', \'' + artist.Person.Name + '\', \'' + artist.Person.DisplayImage + '\');">' + artist.Person.Name + '</a></div>';
            }

        }
        html += '</div>';
        $('#associatedArtists').append(html);
        HideModalLoader('associatedArtist');
    }
    else {
        alert(HandleErrors(obj));
    }
}
function RemoveAssociatedArtist(id) {
    $('#' + id).remove();
}
function SelectArtist(id, name, imageUrl) {
    var html = '<div class="col-md-2 associated-artist" style="text-align:center;" id="' + id + '">' +
                '<input type="hidden" name="artist_id[]" id="artist_id_' + id + '" value="' + id + '" />' +
                '<img src="' + imageUrl + '" width="150" title="' + name + '" alt="' + name + '" /><br />' +
                name + '<br />' +
                '<a href="javascript:;" onclick="RemoveAssociatedArtist(\'' + id + '\');">Remove</a>' +
            '</div>';
    $('#associated-add-button').before(html);
}

function EditMagazineIssue(id)
{
    ShowModalLoader("magazine_issue", "Loading Issue");
    AjaxCall("/ajax/magazine_issue/get", { "magazine_issue_id": id }, "POST", EditMagazineIssueSuccess, AjaxError);
}
function EditMagazineIssueSuccess(data)
{
    obj = JSON.parse(data);
    if (obj.StatusType == "OK") {
        var results = obj.Results.Issues;
        if (results.length > 0) {
            var issue = results[0];
            $('#magazine_issue_id').val(issue.Id);
            $('#magazine_id').val(issue.MagazineId);
            var release_date = issue.ReleaseDate;
            var date_time = release_date.split('T');
            $('#release_date').val(date_time[0]);
            $('#release_date_text').val(issue.ReleaseDateText);
            $('#pdf_title').html(issue.PdfTitle);
            $('#magazine_issueModal').modal('show');
        }
        else {
            alert('Issue not found');
        }
    }
    else {
        alert(HandleErrors(obj));
    }
    HideModalLoader('magazine_issue');
}

function SaveMagazineIssue()
{
    ShowModalLoader("magazine_issue", "Saving Issue");
    var id = $('#magazine_issue_id').val();
    var release_date = $('#release_date').val();
    var release_date_text = $('#release_date_text').val();
    var data = { 'id': id, 'release_date': release_date, 'release_date_text': release_date_text };
    AjaxCall('/ajax/magazine_issues/save', data, "POST", SaveMagazineIssueSuccess, AjaxError);
}

function SaveMagazineIssueSuccess(data)
{
    obj = JSON.parse(data);
    if (obj.StatusType == "OK") {
        HideModalLoader('magazine_issue');
        $('#magazine_issueModal').modal('hide');
        location.reload();
    }
    else {
        HideModalLoader('magazine_issue');
        alert(HandleErrors(obj));
    }
}