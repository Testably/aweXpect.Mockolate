using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using Mockolate;
using Mockolate.Web;
using static aweXpect.Expect;

namespace aweXpect.Mockolate.Aot;

internal static class Checks
{
	private const string ReflectionFallbackSwitch = "aweXpect.ReflectionFallback.IsSupported";

	public static readonly Check[] All =
	[
		new("Once passes for a single interaction",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Once();
			})),
		new("Once fails for two interactions",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				sut.Greet(1);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Once();
			}, "the aweXpect.Mockolate.Aot.IGreeter mock", "invoked method Greet(1) exactly once,", "but found it twice")),
		new("Never passes without an interaction",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(2);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Never();
			})),
		new("Never fails for an interaction",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Never();
			}, "never invoked method Greet(1),", "but found it once")),
		new("AtLeast passes for enough interactions",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				sut.Greet(1);
				await That(sut.Mock.Verify.Greet(It.Is(1))).AtLeastOnce();
				await That(sut.Mock.Verify.Greet(It.Is(1))).AtLeast(2);
			})),
		new("AtLeast fails for too few interactions",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				await That(sut.Mock.Verify.Greet(It.Is(1))).AtLeast(2);
			}, "invoked method Greet(1) at least twice,", "but found it only once")),
		new("Between passes for a count in the range",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				sut.Greet(1);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Between(1).And(3);
			})),
		new("Between fails for a count outside the range",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				await That(sut.Mock.Verify.Greet(It.Is(1))).Between(1).And(3);
			}, "invoked method Greet(1) between 1 and 3 times,", "but never found it")),
		new("Times passes when the predicate holds",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				sut.Greet(1);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Times(n => n % 2 == 0);
			})),
		new("Times fails when the predicate does not hold",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Times(n => n % 2 == 0);
			}, "invoked method Greet(1) according to the predicate n => n % 2 == 0,", "but found it once")),
		new("Then passes for interactions in order",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				sut.Greet(2);
				sut.Greet(3);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Then(m => m.Greet(It.Is(2)), m => m.Greet(It.Is(3)));
			})),
		new("Then fails for interactions out of order",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				sut.Greet(2);
				await That(sut.Mock.Verify.Greet(It.Is(2))).Then(m => m.Greet(It.Is(1)));
			}, "invoked method Greet(2), then", "but it invoked method Greet(1) too early", "All Interactions:")),
		new("Within passes for an interaction in the background",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				Task background = Task.Delay(50).ContinueWith(_ => sut.Greet(1), TaskScheduler.Default);
				await That(sut.Mock.Verify.Greet(It.Is(1))).AtLeastOnce().Within(TimeSpan.FromSeconds(30));
				await background;
			})),
		new("Within fails for a missing interaction after the timeout",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				await That(sut.Mock.Verify.Greet(It.Is(1))).Once().Within(TimeSpan.FromMilliseconds(50));
			}, "invoked method Greet(1) exactly once within 0:00.050,", "but never found it")),
		new("an awaitable subject passes for an interaction in the background",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				Task background = Task.Delay(50).ContinueWith(_ => sut.Greet(1), TaskScheduler.Default);
				await That(sut.Mock.Verify.Greet(It.Is(1)).Within(TimeSpan.FromSeconds(30))).Once();
				await background;
			})),
		new("an awaitable subject fails for a missing interaction after its timeout",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				await That(sut.Mock.Verify.Greet(It.Is(1)).Within(TimeSpan.FromMilliseconds(50))).Once();
			}, "invoked method Greet(1) exactly once,", "but never found it")),
		new("AllInteractionsAreVerified passes when every interaction is verified",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				sut.Greet(2);
				await That(sut.Mock.Verify.Greet(It.IsAny<int>())).Twice();
				await That(sut.Mock.Verify).AllInteractionsAreVerified();
			})),
		new("AllInteractionsAreVerified fails for an unverified interaction",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Greet(1);
				sut.Greet(2);
				await That(sut.Mock.Verify.Greet(It.Is(1))).Once();
				await That(sut.Mock.Verify).AllInteractionsAreVerified();
			}, "has all interactions verified,", "interaction was not verified:", "Greet(2)")),
		new("AllSetupsAreUsed passes when every setup is used",
			() => ShouldPass(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Mock.Setup.Greet(It.Is(1));
				sut.Greet(1);
				await That(sut.Mock.Verify).AllSetupsAreUsed();
			})),
		new("AllSetupsAreUsed fails for an unused setup",
			() => ShouldFail(async () =>
			{
				IGreeter sut = IGreeter.CreateMock();
				sut.Mock.Setup.Greet(It.Is(1));
				sut.Mock.Setup.Greet(It.Is(2));
				sut.Greet(1);
				await That(sut.Mock.Verify).AllSetupsAreUsed();
			}, "has used all setups,", "setup was not used:", "Greet(2)")),
		new("WithJson passes for an equivalent body",
			() => ShouldPass(async () =>
			{
				HttpClient httpClient = await PostAsync("{\"foo\": 1, \"bar\": \"baz\"}");
				await That(httpClient.Mock.Verify.PostAsync(It.IsAny<Uri>(),
					It.IsHttpContent().WithJson("{\"bar\": \"baz\", \"foo\": 1}"))).Once();
			})),
		new("WithJson fails for a differing body",
			() => ShouldFail(async () =>
			{
				HttpClient httpClient = await PostAsync("{\"foo\": 2}");
				await That(httpClient.Mock.Verify.PostAsync(It.IsAny<Uri>(),
					It.IsHttpContent().WithJson("{\"foo\": 1}"))).Once();
			}, "exactly once,", "but never found it")),
		new("WithJsonMatching passes for an equivalent body or fails loudly",
			() => ShouldPassOrFailLoudly(async () =>
			{
				HttpClient httpClient = await PostAsync("{\"foo\": 1}");
				await That(httpClient.Mock.Verify.PostAsync(It.IsAny<Uri>(),
					It.IsHttpContent().WithJsonMatching(new { foo = 1, }))).Once();
			})),
		new("WithJsonMatching fails for a differing body or fails loudly",
			() => ShouldFailOrFailLoudly(async () =>
			{
				HttpClient httpClient = await PostAsync("{\"foo\": 2}");
				await That(httpClient.Mock.Verify.PostAsync(It.IsAny<Uri>(),
					It.IsHttpContent().WithJsonMatching(new { foo = 1, }))).Once();
			}, "exactly once,", "but never found it")),
	];

	private static async Task<HttpClient> PostAsync(string body)
	{
		HttpClient httpClient = HttpClient.CreateMock();
		httpClient.Mock.Setup.PostAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>())
			.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
		await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(body), CancellationToken.None);
		return httpClient;
	}

	private static async Task<string?> ShouldPass(Func<Task> act)
	{
		try
		{
			await act();
			return null;
		}
		catch (Exception exception)
		{
			return $"threw {exception.GetType().FullName}: {exception.Message}";
		}
	}

	/// <summary>
	///     Expects the failure exception of aweXpect without a test framework, whose message contains every part.
	/// </summary>
	private static async Task<string?> ShouldFail(Func<Task> act, params string[] parts)
	{
		try
		{
			await act();
		}
		catch (FailException exception)
		{
			string? missing = Array.Find(parts, part => !exception.Message.Contains(part, StringComparison.Ordinal));
			return missing is null ? null : $"message lacks \"{missing}\": {exception.Message}";
		}
		catch (Exception exception)
		{
			return $"threw {exception.GetType().FullName} instead of {typeof(FailException).FullName}: {exception.Message}";
		}

		return "did not throw";
	}

	/// <remarks>
	///     The expected value is serialized by reflection, which works where the fallback is supported and has to
	///     fail with the error naming the switch otherwise.
	/// </remarks>
	private static Task<string?> ShouldPassOrFailLoudly(Func<Task> act)
		=> ReflectionFallback.IsSupported ? ShouldPass(act) : ShouldFailLoudly(act);

	/// <inheritdoc cref="ShouldPassOrFailLoudly(Func{Task})" />
	private static Task<string?> ShouldFailOrFailLoudly(Func<Task> act, params string[] parts)
		=> ReflectionFallback.IsSupported ? ShouldFail(act, parts) : ShouldFailLoudly(act);

	private static async Task<string?> ShouldFailLoudly(Func<Task> act)
	{
		try
		{
			await act();
		}
		catch (NotSupportedException exception)
			when (exception.Message.Contains(ReflectionFallbackSwitch, StringComparison.Ordinal))
		{
			return null;
		}
		catch (Exception exception)
		{
			return $"threw {exception.GetType().FullName} without naming the switch: {exception.Message}";
		}

		return "did not throw";
	}
}
