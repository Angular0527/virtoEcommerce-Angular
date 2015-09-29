﻿if ((typeof VirtoCommerce) == "undefined") {
    var VirtoCommerce = {};
}

Array.prototype.getElementByVal = function (propName, propValue) {
    var el = null;
    for (var i = 0; i < this.length; i++) {
        if (this[i][propName] == propValue) {
            el = this[i];
            break;
        }
    }
    return el;
}

VirtoCommerce.changeCurrency = function (currencyCode) {
    VirtoCommerce.redirect(location.href, { currency: currencyCode });
};

VirtoCommerce.redirect = function(url, params) {

    url = url || window.location.href || '';

    if (params != undefined) {
        url = url.match(/\?/) ? url : url + '?';

        for (var key in params) {
            var re = RegExp(';?' + key + '=?[^&;]*', 'g');
            url = url.replace(re, '');
            url += '&' + key + '=' + params[key];
        }
    }

    // cleanup url 
    url = url.replace(/[;&]$/, '');
    url = url.replace(/\?[;&]/, '?');
    url = url.replace(/[;&]{2}/g, '&');
    window.location.replace(url);
};

VirtoCommerce.url = function (url) {
    //var baseUrl = window.location.protocol + "//" + location.host;
    var baseUrl = $("base").attr("href");
    return baseUrl + url;
};

VirtoCommerce.renderDynamicContent = function () {
    var url = VirtoCommerce.url("/banners");
    var placeholders = $("[data-vccontentid]");
    var placeholderIds = new Array();
    if (placeholders.length) {
        url += "?";
        for (var i = 0; i < placeholders.length; i++) {
            var placeholderId = $(placeholders[i]).data("vccontentid");
            placeholderIds.push(placeholderId);
            url += "placenames=" + placeholderId;
            if (i < placeholders.length - 1) {
                url += "&";
            }
        }

        $.get(url, function(htmlResponse) {
            if (htmlResponse) {
                var htmlData = $("<div />").html(htmlResponse);
                for (var i = 0; i < placeholderIds.length; i++) {
                    var htmlPlaceContent = htmlData.find("#" + placeholderIds[i]).html();
                    $("[data-vccontentid='" + placeholderIds[i] + "']").html(htmlPlaceContent);
                }

                $(".flexslider").flexslider();
            }
        });
    }
};

$(function () {
    VirtoCommerce.renderDynamicContent();

    $("#addToQuote").on("click", function () {
        $.ajax({
            type: "POST",
            url: VirtoCommerce.url("/quote/add"),
            data: {
                variantId: $("#productSelect").val()
            },
            success: function (jsonResult) {
                if (jsonResult) {
                    var quoteCount = $("#quoteCount");
                    quoteCount.text(jsonResult.items_count);
                    if (jsonResult.items_count > 0) {
                        quoteCount.removeClass("hidden-count");
                    } else {
                        quoteCount.addClass("hidden-count");
                    }
                }
            }
        });
    });

    if (typeof defaultCustomerAddress != "undefined" && defaultCustomerAddress) {
        $("#actual-quote-request-shipping-address-first-name").val(defaultCustomerAddress.first_name);
        $("#actual-quote-request-shipping-address-last-name").val(defaultCustomerAddress.last_name);
        $("#actual-quote-request-shipping-address-country").val(defaultCustomerAddress.country);
        $("#actual-quote-request-shipping-address-province").val(defaultCustomerAddress.province);
        $("#actual-quote-request-shipping-address-city").val(defaultCustomerAddress.city);
        $("#actual-quote-request-shipping-address-company").val(defaultCustomerAddress.company);
        $("#actual-quote-request-shipping-address-address1").val(defaultCustomerAddress.address1);
        $("#actual-quote-request-shipping-address-address2").val(defaultCustomerAddress.address2);
        $("#actual-quote-request-shipping-address-zip").val(defaultCustomerAddress.zip);
        $("#actual-quote-request-shipping-address-phone").val(defaultCustomerAddress.phone);

        $("#actual-quote-request-billing-address-first-name").val(defaultCustomerAddress.first_name);
        $("#actual-quote-request-billing-address-last-name").val(defaultCustomerAddress.last_name);
        $("#actual-quote-request-billing-address-country").val(defaultCustomerAddress.country);
        $("#actual-quote-request-billing-address-province").val(defaultCustomerAddress.province);
        $("#actual-quote-request-billing-address-city").val(defaultCustomerAddress.city);
        $("#actual-quote-request-billing-address-company").val(defaultCustomerAddress.company);
        $("#actual-quote-request-billing-address-address1").val(defaultCustomerAddress.address1);
        $("#actual-quote-request-billing-address-address2").val(defaultCustomerAddress.address2);
        $("#actual-quote-request-billing-address-zip").val(defaultCustomerAddress.zip);
        $("#actual-quote-request-billing-address-phone").val(defaultCustomerAddress.phone);
    }

    if ($("#actual-quote-request-shipping-quote").is(":checked")) {
        $("#shipping-address-block").show();
        if (!$("#actual-quote-request-same-billing-address").is(":checked")) {
            $("#billing-address-block").show();
        }
    }

    $("#actual-quote-request-shipping-quote").on("change", function () {
        var checkbox = $(this);
        if (checkbox.is(":checked")) {
            new Shopify.CountryProvinceSelector("actual-quote-request-shipping-address-country", "actual-quote-request-shipping-address-province", {});
            $("#shipping-address-block").show();
            if (!$("#actual-quote-request-same-billing-address").is(":checked")) {
                new Shopify.CountryProvinceSelector("actual-quote-request-billing-address-country", "actual-quote-request-billing-address-province", {});
                $("#billing-address-block").show();
            }
        } else {
            $("#shipping-address-block").hide();
            $("#billing-address-block").hide();
        }
    });

    $("#actual-quote-request-same-billing-address").on("change", function () {
        var checkbox = $(this);
        if (!checkbox.is(":checked")) {
            new Shopify.CountryProvinceSelector("actual-quote-request-billing-address-country", "actual-quote-request-billing-address-province", { });
            $("#billing-address-block").show();
        } else {
            $("#billing-address-block").hide();
        }
    });

    $("body").delegate(".cart-row.quote .js-qty .js--add", "click", function () {
        var id = $(this).data("id");
        var quantityInput = $(this).parents(".js-qty").find("input");
        var quantity = parseInt(quantityInput.val()) + 1;
        quantityInput.val(quantity);
    });

    $("body").delegate(".cart-row.quote .js-qty .js--minus", "click", function () {
        var id = $(this).data("id");
        var quantityInput = $(this).parents(".js-qty").find("input");
        var quantity = parseInt(quantityInput.val()) - 1;
        if (quantity >= 1) {
            quantityInput.val(quantity);
        }
    });

    $(".add-tier").on("click", function (event) {
        event.preventDefault();
        var defaultTierPrice = $(this).parents(".cart-row.quote").data("default-tier-price");

        var tierHtml = "<div class=\"js-qty\">";
        tierHtml += "<div class=\"js-qty--inner\">";
        tierHtml += "<input class=\"js--num\" pattern=\"[0-9]*\" type=\"text\" value=\"1\" />";
        tierHtml += "<input class=\"js--price\" type=\"hidden\" value=\"" + defaultTierPrice + "\" />";
        tierHtml += "<span class=\"js--qty-adjuster js--add\">+</span>";
        tierHtml += "<span class=\"js--qty-adjuster js--minus\">-</span>";
        tierHtml += "</div>";
        tierHtml += "<a class=\"link-action\">Remove</a>";
        tierHtml += "</div>";

        var qtyCount = $(this).parents(".grid-item").find(".js-qty").length - 1;
        var predLastQty = $(this).parents(".grid-item").find(".js-qty:eq(" + (qtyCount - 1) + ")");

        predLastQty.after(tierHtml);
    });

    $("body").delegate(".js-qty .link-action", "click", function () {
        var proposalPriceIndex = $(this).parents(".js-qty").data("for-proposal-price");
        $(this).parents(".grid-item").siblings(".grid-item.proposal-prices").find("[data-proposal-price='" + proposalPriceIndex + "']").remove();
        $(this).parents(".js-qty").remove();
    });

    $("#btn-submit-quote-request").on("click", function () {
        var quoteRequest = {
            Id: $("#actual-quote-request-id").val(),
            Comment: $("#actual-quote-request-comment").val(),
            Email: $("#actual-quote-request-email").val(),
            Items: []
        };
        $.each($(".cart-row.quote"), function () {
            var itemElement = $(this);
            var quoteItem = {
                Id: itemElement.data("id"),
                Comment: itemElement.find(".quote_item_comment").val(),
                ProposalPrices: []
            };

            $.each(itemElement.find(".js--num"), function () {
                var proposalPriceElement = $(this).siblings(".js--price");
                tierPrice = {
                    Quantity: parseInt($(this).val()),
                    Price: proposalPriceElement.val()
                };

                quoteItem.ProposalPrices.push(tierPrice);
            });

            quoteRequest.Items.push(quoteItem);
        });

        if ($("#shipping-address-block").is(":visible")) {
            quoteRequest.ShippingAddress = {
                FirstName: $("#actual-quote-request-shipping-address-first-name").val(),
                LastName: $("#actual-quote-request-shipping-address-last-name").val(),
                Country: $("#actual-quote-request-shipping-address-country").val(),
                Province: $("#actual-quote-request-shipping-address-province").val(),
                City: $("#actual-quote-request-shipping-address-city").val(),
                Company: $("#actual-quote-request-shipping-address-company").val(),
                Address1: $("#actual-quote-request-shipping-address-address1").val(),
                Address2: $("#actual-quote-request-shipping-address-address2").val(),
                Zip: $("#actual-quote-request-shipping-address-zip").val(),
                Phone: $("#actual-quote-request-shipping-address-phone").val()
            };
            if ($("#actual-quote-request-same-billing-address").is(":checked")) {
                quoteRequest.BillingAddress = quoteRequest.ShippingAddress;
            } else {
                quoteRequest.BillingAddress = {
                    FirstName: $("#actual-quote-request-billing-address-first-name").val(),
                    LastName: $("#actual-quote-request-billing-address-last-name").val(),
                    Country: $("#actual-quote-request-billing-address-country").val(),
                    Province: $("#actual-quote-request-billing-address-province").val(),
                    City: $("#actual-quote-request-billing-address-city").val(),
                    Company: $("#actual-quote-request-billing-address-company").val(),
                    Address1: $("#actual-quote-request-billing-address-address1").val(),
                    Address2: $("#actual-quote-request-billing-address-address2").val(),
                    Zip: $("#actual-quote-request-billing-address-zip").val(),
                    Phone: $("#actual-quote-request-billing-address-phone").val()
                };
            };
        };

        $.ajax({
            type: "POST",
            url: VirtoCommerce.url("/quote/submit"),
            data: quoteRequest,
            success: function (jsonResult) {
                if (jsonResult) {
                    if (jsonResult.error_message) {
                        alert(jsonResult.error_message);
                    } else {
                        window.location.href = jsonResult.redirect_url;
                    }
                }
            }
        });
    });

    $("#btn-checkout-quote-request").on("click", function () {
        var quoteRequest = getQuoteRequest();
        $.ajax({
            type: "POST",
            url: VirtoCommerce.url("/account/quote/checkout"),
            data: quoteRequest,
            success: function (jsonResponse) {
                if (jsonResponse) {
                    window.location.href = jsonResponse.redirect_url;
                }
            }
        });
    });

    $(".proposal-price-radio").on("change", function () {
        var quoteRequest = getQuoteRequest();
        recalculateQuoteRequestTotals(quoteRequest);
    });
});

var getQuoteRequest = function () {
    var quoteRequest = {
        Number: $("#quote-request-number").data("number"),
        Items: []
    };
    $.each($(".cart-row.quote"), function () {
        var selectedProposalPrice = $(this).find(".proposal-price-radio:checked");
        var quoteItem = {
            Id: $(this).data("id"),
            SelectedTierPrice: {
                Quantity: selectedProposalPrice.data("quantity"),
                Price: selectedProposalPrice.val()
            }
        };
        quoteRequest.Items.push(quoteItem);
    });
    return quoteRequest;
}

var recalculateQuoteRequestTotals = function (quoteRequest) {
    $.ajax({
        type: "POST",
        url: VirtoCommerce.url("/quote/recalculate"),
        data: quoteRequest,
        success: function (jsonResponse) {
            $("#quote-subtotal").text(jsonResponse.totals.sub_total_exl_tax);
            $("#quote-tax-total").text(jsonResponse.totals.tax_total);
            $("#quote-grand-total").text(jsonResponse.totals.grand_total_incl_tax);
        }
    });
}